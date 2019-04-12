using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using UnityEditorInternal;


public class LODExtendedUtility
{    
    static public GameObject GetParentLODGroupGameObject(GameObject GO)
    {
        LODGroup LODGroupParent = GO.GetComponentInParent<LODGroup>();
        if (LODGroupParent == null)
            return null;
        LOD[] LODs = LODGroupParent.GetLODs();
        var FoundLOD = LODs.Where(lod => lod.renderers.Where(renderer => renderer == GO.GetComponent<Renderer>()).ToArray().Count() > 0).ToArray();
        if (FoundLOD != null && FoundLOD.Count() > 0)
            return (LODGroupParent.gameObject);

        return null;
    }

    static public LODGroup GetParentLODGroupComponent(GameObject GO)
    {
        LODGroup LODGroupParent = GO.GetComponentInParent<LODGroup>();
        if (LODGroupParent == null)
            return null;
        LOD[] LODs = LODGroupParent.GetLODs();

        var FoundLOD = LODs.Where(lod => lod.renderers.Where(renderer => renderer == GO.GetComponent<Renderer>()).ToArray().Count() > 0).ToArray();
        if (FoundLOD != null && FoundLOD.Count() > 0)
            return (LODGroupParent);

        return null;
    }

    static public int GetLODid(GameObject GO)
    {
        LODGroup LODGroupParent = GO.GetComponentInParent<LODGroup>();
        if (LODGroupParent == null)
            return -1;
        LOD[] LODs = LODGroupParent.GetLODs();

        var index = Array.FindIndex(LODs, lod => lod.renderers.Where(renderer => renderer == GO.GetComponent<Renderer>()).ToArray().Count() > 0);        
        return index;
    }

    public static int GetVisibleLODSceneView(LODGroup lodGroup)
    {
        Camera camera = SceneView.lastActiveSceneView.camera;
        return GetVisibleLOD(lodGroup,camera);
    }

    public static int GetVisibleLOD(LODGroup lodGroup, Camera camera = null)
    {
        var lods = lodGroup.GetLODs();
        var relativeHeight = GetRelativeHeight(lodGroup,camera ?? Camera.current);

        var lodIndex = GetVisibleLOD(lods, GetMaxLOD(lodGroup), relativeHeight, camera);

        return lodIndex;
    }

    static float GetRelativeHeight(LODGroup lodGroup, Camera camera)
    {
        var distance = (lodGroup.transform.TransformPoint(lodGroup.localReferencePoint) - camera.transform.position).magnitude;
        return DistanceToRelativeHeight(camera, (distance/QualitySettings.lodBias), GetWorldSpaceSize(lodGroup));
    }

    static float DistanceToRelativeHeight(Camera camera, float distance, float size)
    {
        if (camera.orthographic)
            return size * 0.5F / camera.orthographicSize;

        var halfAngle = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5F);
        var relativeHeight = size * 0.5F / (distance * halfAngle);
        return relativeHeight;
    }
    public static int GetMaxLOD(LODGroup lodGroup)
    {
        return lodGroup.lodCount - 1;
    }
    public static float GetWorldSpaceSize(LODGroup lodGroup)
    {
        return GetWorldSpaceScale(lodGroup.transform) * lodGroup.size;
    }
    static float GetWorldSpaceScale(Transform t)
    {
        var scale = t.lossyScale;
        float largestAxis = Mathf.Abs(scale.x);
        largestAxis = Mathf.Max(largestAxis, Mathf.Abs(scale.y));
        largestAxis = Mathf.Max(largestAxis, Mathf.Abs(scale.z));
        return largestAxis;
    }

    static int GetVisibleLOD(LOD[] lods, int maxLOD, float relativeHeight, Camera camera = null)
    {
        var lodIndex = maxLOD;

        for (var i = 0; i < lods.Length; i++)
        {
            var lod = lods[i];

            if (relativeHeight >= lod.screenRelativeTransitionHeight)
            {
                lodIndex = i;
                break;
            }
        }

        return lodIndex;
    }
}
