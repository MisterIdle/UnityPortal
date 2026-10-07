using UnityEngine;

public static class PortalMathUtility
{
    private static readonly Matrix4x4 HalfTurnMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0));
    private static readonly Quaternion HalfTurnRotation = Quaternion.Euler(0, 180, 0);

    public static Matrix4x4 GetTransitionMatrix(Transform inPortal, Transform outPortal)
    {
        return outPortal.localToWorldMatrix * HalfTurnMatrix * inPortal.worldToLocalMatrix;
    }

    public static Vector3 TransformPosition(Vector3 position, Transform inPortal, Transform outPortal)
    {
        return GetTransitionMatrix(inPortal, outPortal).MultiplyPoint3x4(position);
    }

    public static Quaternion TransformRotation(Quaternion rotation, Transform inPortal, Transform outPortal)
    {
        return outPortal.rotation * HalfTurnRotation * Quaternion.Inverse(inPortal.rotation) * rotation;
    }

    public static Vector4 CalculateClipPlaneSpace(Camera portalCamera, Vector3 clipPos, Vector3 clipNormal)
    {
        Matrix4x4 camSpaceMatrix = portalCamera.worldToCameraMatrix;
        
        Vector3 camSpacePos = camSpaceMatrix.MultiplyPoint(clipPos);
        Vector3 camSpaceNormal = camSpaceMatrix.MultiplyVector(clipNormal).normalized;

        return new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, -Vector3.Dot(camSpacePos, camSpaceNormal));
    }
}