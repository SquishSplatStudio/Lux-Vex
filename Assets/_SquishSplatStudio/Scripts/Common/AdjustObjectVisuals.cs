using UnityEngine;

namespace SquishSplatStudio
{
    public class AdjustObjectVisuals : MonoBehaviour
    {
        public bool StandardMaterial;
        public Material AliveMaterial;
        public Material DeadMaterial;
        MeshRenderer myMR;
        float startIntensity = 1f;
        public Material startMaterial;
        bool _initialized;

        public void Initialize()
        {
            myMR = GetComponent<MeshRenderer>();

            if (!StandardMaterial)
                if(myMR.material.HasProperty("_glowStrength"))
                    startIntensity = myMR.material.GetFloat("_glowStrength");

            _initialized = true;
        }

        public void AdjustMaterials(float newLerpPercent, bool isDying)
        {
            // Error Catch
            if (!_initialized)
                Initialize();

            float standardColourLerp;
            float lerpDirection;
            // Inverted Colour Lerp Value
            if (!isDying)
            {
                standardColourLerp = 1f * newLerpPercent;
                lerpDirection = 1f;
            }
            else
            {
                standardColourLerp = 1f - (1f * newLerpPercent);
                lerpDirection = 0f;
            }

            // Do Standard Material Changes
            if (StandardMaterial && AliveMaterial != null && DeadMaterial != null)
            {
                Color changeColour;
                Color changeEmission;

                if (!isDying)
                {
                    changeColour = AliveMaterial.GetColor("_BaseColor");
                    changeEmission = AliveMaterial.GetColor("_EmissionColor");
                }
                else
                {
                    changeColour = DeadMaterial.GetColor("_BaseColor");
                    changeEmission = DeadMaterial.GetColor("_EmissionColor");
                }

                Color _newEmissionColor = Color.Lerp(changeEmission, changeColour, standardColourLerp);

                if (myMR != null && myMR.material.HasProperty("_BaseColor") && myMR.material.HasProperty("_EmissionColor") && myMR.material.HasProperty("_Smoothness"))
                {
                    Color _currentColor = myMR.material.GetColor("_BaseColor");
                    myMR.material.SetColor("_BaseColor", Color.Lerp(_currentColor, changeColour, standardColourLerp));
                    myMR.material.SetColor("_EmissionColor", _newEmissionColor);
                    myMR.material.SetFloat("_Smoothness", Mathf.Lerp(myMR.material.GetFloat("_Smoothness"), lerpDirection, standardColourLerp));
                }
            }

            // Do Standard with no Alive or Dead Material
            if (StandardMaterial && AliveMaterial == null && DeadMaterial == null)
            {
                Color changeColour;
                Color changeEmission;

                if (!isDying)
                {
                    changeColour = startMaterial.GetColor("_BaseColor");
                    changeEmission = startMaterial.GetColor("_EmissionColor");
                }
                else
                {
                    changeColour = new Color(0.1f, 0.1f, 0.1f, 0.1f);
                    changeEmission = Color.black;
                }

                Color _newEmissionColor = Color.Lerp(changeEmission, changeColour, standardColourLerp);

                if (myMR != null && myMR.material.HasProperty("_BaseColor") && myMR.material.HasProperty("_EmissionColor") && myMR.material.HasProperty("_Smoothness"))
                {
                    Color _currentColor = myMR.material.GetColor("_BaseColor");
                    myMR.material.SetColor("_BaseColor", Color.Lerp(_currentColor, changeColour, standardColourLerp));
                    myMR.material.SetColor("_EmissionColor", _newEmissionColor);
                    myMR.material.SetFloat("_Smoothness", Mathf.Lerp(myMR.material.GetFloat("_Smoothness"), lerpDirection, standardColourLerp));
                }
            }

            // Do Shader Material
            if (!StandardMaterial)
            {
                if(myMR != null)
                    if (myMR.material.HasProperty("_glowStrength"))
                        myMR.material.SetFloat("_glowStrength", startIntensity * newLerpPercent);
            }
        }
    }
}