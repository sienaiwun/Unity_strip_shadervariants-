using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.LWRP
{
    internal class KillShaderVariantsPreprocessor : IPreprocessShaders
    {
        private string YText(string text) { return "<color=#ff0>" + text + "</color>"; }

        int m_TotalVariantsInputCount;
        int m_TotalVariantsOutputCount;

        // Multiple callback may be implemented.
        // The first one executed is the one where callbackOrder is returning the smallest number.
        public int callbackOrder { get { return 1; } }


        bool StripGlobalShader(string shaderName, Shader shader)
        {
            if (shader.name.Equals(shaderName))
                return true;
            return false;
        }

        bool StripGlobalKeywords(string keyword, ShaderCompilerData compilerData)
        {
            if (compilerData.shaderKeywordSet.IsEnabled(new ShaderKeyword(keyword)))
                return true;
            return false;
        }

       bool StripSpecifiedKeyward(SpecifiedShaderKeyword specifed, Shader shader, ShaderCompilerData compilerData)
        {
            if (shader.name.Equals(specifed.shadername))
            {
                foreach(string keyword in specifed.keywords)
                {
                    if (compilerData.shaderKeywordSet.IsEnabled(new ShaderKeyword(keyword)))
                        return true;
                }
            }
            return false;
        }

        bool StripUnused(StripVariantsInfo stripInfo/*not null*/, Shader shader, ShaderSnippetData snippetData, ShaderCompilerData compilerData)
        {
            foreach(string shadername in stripInfo.globalShaderNames)
                if (StripGlobalShader(shadername, shader))
                    return true;
            foreach (string keyword in stripInfo.globalKeywords)
                if (StripGlobalKeywords(keyword, compilerData))
                    return true;
                  
            if (stripInfo.specifiedShaders!=null)
                foreach (SpecifiedShaderKeyword specifed in stripInfo.specifiedShaders)
                    if (StripSpecifiedKeyward(specifed, shader, compilerData))
                        return true;
            return false;
        }

        void LogShaderVariants(Shader shader, ShaderSnippetData snippetData, ShaderVariantLogLevel logLevel, int prevVariantsCount, int currVariantsCount)
        {
            Func<bool> showlog = () =>
            {
                if (logLevel == ShaderVariantLogLevel.AllShaders)
                    return true;
                if (logLevel == ShaderVariantLogLevel.OnlyLightweightRPShaders && shader.name.Contains("Lightweight Render Pipeline"))
                    return true;
                LightweightRenderPipelineAsset lwrpAsset = GraphicsSettings.renderPipelineAsset as LightweightRenderPipelineAsset;
                if (logLevel == ShaderVariantLogLevel.ShowIfAbove && currVariantsCount > lwrpAsset.shaderVariantShowNum)
                    return true;
                return false;
            };
            if (showlog())
            {
                float percentageCurrent = (float)currVariantsCount / (float)prevVariantsCount * 100f;
                float percentageTotal = (float)m_TotalVariantsOutputCount / (float)m_TotalVariantsInputCount * 100f;
               
                string result = string.Format("{9}'s STRIPPING: {0} ({1} pass) ({2}) -" +
                        " Remaining shader variants = {3}/{4} = {5}% ||| Total = {6}/{7} = {8}%",
                        shader.name, snippetData.passName, snippetData.shaderType.ToString(), currVariantsCount,
                        prevVariantsCount, percentageCurrent, m_TotalVariantsOutputCount, m_TotalVariantsInputCount,
                        percentageTotal, Application.productName);
                Debug.Log(YText(result));
            }
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippetData, IList<ShaderCompilerData> compilerDataList)
        {
            LightweightRenderPipelineAsset lwrpAsset = GraphicsSettings.renderPipelineAsset as LightweightRenderPipelineAsset;
            if (lwrpAsset == null || compilerDataList == null || compilerDataList.Count == 0)
                return;
            StripVariantsInfo stripInfo = lwrpAsset.stripVariantsInfo;
           

            int prevVariantCount = compilerDataList.Count;

            for (int i = 0; i < compilerDataList.Count; ++i)
            {
                if (stripInfo != null)
                {
                    if (StripUnused(stripInfo, shader, snippetData, compilerDataList[i]))
                    {
                        compilerDataList.RemoveAt(i);
                        --i;
                    }
                }
            }
            if (lwrpAsset.shaderVariantLogLevel != ShaderVariantLogLevel.Disabled)
            {
                m_TotalVariantsInputCount += prevVariantCount;
                m_TotalVariantsOutputCount += compilerDataList.Count;
                if (compilerDataList.Count< prevVariantCount)
                    LogShaderVariants(shader, snippetData, lwrpAsset.shaderVariantLogLevel, prevVariantCount, compilerDataList.Count);
            }
        }

    }
}
