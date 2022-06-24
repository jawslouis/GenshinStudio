using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AssetStudio;
using Quaternion = AssetStudio.Quaternion;
using Vector3 = AssetStudio.Vector3;

namespace AssetStudioGUI
{

    public class TeapotExporter
    {

        public static string ExportFolder = Directory.GetCurrentDirectory() + "/Extract/";
        public static HashSet<string> Exported = new HashSet<string>();
        public static HashSet<string> NullMeshes = new HashSet<string>();

        public static List<string> Output = new List<string>();

        public ImportedFrame RootFrame { get; protected set; }
        public List<ImportedMesh> MeshList { get; protected set; } = new List<ImportedMesh>();
        public List<ImportedMaterial> MaterialList { get; protected set; } = new List<ImportedMaterial>();
        public List<ImportedTexture> TextureList { get; protected set; } = new List<ImportedTexture>();

        private Dictionary<Texture2D, string> textureNameDictionary = new Dictionary<Texture2D, string>();
        private Dictionary<Transform, ImportedFrame> transformDictionary = new Dictionary<Transform, ImportedFrame>();

        private List<string> MeshNames = new List<string>();

        private string RootName = "";


        public TeapotExporter(GameObject m_GameObject)
        {

            RootName = m_GameObject.m_Name;

            var m_Transform = m_GameObject.m_Transform;


            var frameList = new List<ImportedFrame>();
            var tempTransform = m_Transform;
            while (tempTransform.m_Father.TryGet(out var m_Father))
            {
                frameList.Add(ConvertTransform(m_Father));
                tempTransform = m_Father;
            }
            if (frameList.Count > 0)
            {
                RootFrame = frameList[frameList.Count - 1];
                for (var i = frameList.Count - 2; i >= 0; i--)
                {
                    var frame = frameList[i];
                    var parent = frameList[i + 1];
                    parent.AddChild(frame);
                }
                ConvertTransforms(m_Transform, frameList[0]);
            }
            else
            {
                ConvertTransforms(m_Transform, null);
            }

            ConvertMeshRenderer(m_Transform);
        }


        private void ConvertMeshRenderer(Transform m_Transform)
        {
            m_Transform.m_GameObject.TryGet(out var m_GameObject);

            Output.Add($"ConvertMeshRenderer: {m_GameObject.m_Name}");

            if (m_GameObject.m_MeshRenderer != null)
            {
                ProcessMeshRenderer(m_GameObject.m_MeshRenderer);
            }

            if (m_GameObject.m_SkinnedMeshRenderer != null)
            {
                ProcessMeshRenderer(m_GameObject.m_SkinnedMeshRenderer);
            }

            foreach (var pptr in m_Transform.m_Children)
            {
                if (pptr.TryGet(out var child))
                    ConvertMeshRenderer(child);
            }

        }


        private static Mesh GetMesh(Renderer meshR)
        {
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                if (sMesh.m_Mesh.TryGet(out var m_Mesh))
                {
                    return m_Mesh;
                }
            }
            else
            {
                meshR.m_GameObject.TryGet(out var m_GameObject);
                if (m_GameObject.m_MeshFilter != null)
                {
                    if (m_GameObject.m_MeshFilter.m_Mesh.TryGet(out var m_Mesh))
                    {
                        return m_Mesh;
                    }
                }
            }

            return null;
        }

        private string GetTransformPath(Transform transform)
        {
            if (transformDictionary.TryGetValue(transform, out var frame))
            {
                return frame.Path;
            }
            return null;
        }


        private void ProcessMeshRenderer(Renderer meshR)
        {
            meshR.m_GameObject.TryGet(out var m_GameObject2);

            Output.Add($"ProcessMeshRenderer: {m_GameObject2.m_Name}");

            var mesh = GetMesh(meshR);
            if (mesh == null)
            {

                string nullName = "null";
                if (m_GameObject2 != null)
                {
                    nullName = m_GameObject2.m_Name;
                }

                Output.Add($"Mesh is null: {nullName}");

                if (!NullMeshes.Contains(nullName))
                {
                    NullMeshes.Add(nullName);
                }

                return;
            }

            if (!Exported.Contains(mesh.m_Name))
            {
                var asset = new AssetItem(mesh);
                asset.Text = mesh.m_Name;
                Exporter.ExportMesh(asset, ExportFolder + "Mesh/");
                Output.Add($"Exported mesh: {mesh.m_Name}");
                Exported.Add(mesh.m_Name);
            }




            var meshName = mesh.m_Name;

            var iMesh = new ImportedMesh();
            iMesh.Path = GetTransformPath(m_GameObject2.m_Transform);

            iMesh.SubmeshList = new List<ImportedSubmesh>();
            var subHashSet = new HashSet<int>();

            int firstSubMesh = 0;
            if (meshR.m_StaticBatchInfo?.subMeshCount > 0)
            {
                firstSubMesh = meshR.m_StaticBatchInfo.firstSubMesh;
                var finalSubMesh = meshR.m_StaticBatchInfo.firstSubMesh + meshR.m_StaticBatchInfo.subMeshCount;
                for (int i = meshR.m_StaticBatchInfo.firstSubMesh; i < finalSubMesh; i++)
                {
                    subHashSet.Add(i);
                }
            }
            else if (meshR.m_SubsetIndices?.Length > 0)
            {
                firstSubMesh = (int)meshR.m_SubsetIndices.Min(x => x);
                foreach (var index in meshR.m_SubsetIndices)
                {
                    subHashSet.Add((int)index);
                }

            }



            int firstFace = 0;
            for (int i = 0; i < mesh.m_SubMeshes.Length; i++)
            {

                int numFaces = (int)mesh.m_SubMeshes[i].indexCount / 3;
                if (subHashSet.Count > 0 && !subHashSet.Contains(i))
                {
                    firstFace += numFaces;
                    continue;
                }
                var submesh = mesh.m_SubMeshes[i];
                var iSubmesh = new ImportedSubmesh();
                Material mat = null;
                if (i - firstSubMesh < meshR.m_Materials.Length)
                {
                    if (meshR.m_Materials[i - firstSubMesh].TryGet(out var m_Material))
                    {
                        mat = m_Material;
                    }
                }

                ImportedMaterial iMat = ConvertMaterial(mat);

            }

        }


        private ImportedMaterial ConvertMaterial(Material mat)
        {
            ImportedMaterial iMat;
            if (mat != null)
            {
                iMat = ImportedHelpers.FindMaterial(mat.m_Name, MaterialList);
                if (iMat != null)
                {
                    return iMat;
                }
                iMat = new ImportedMaterial();
                iMat.Name = mat.m_Name;

                //textures
                iMat.Textures = new List<ImportedMaterialTexture>();
                foreach (var texEnv in mat.m_SavedProperties.m_TexEnvs)
                {
                    if (!texEnv.Value.m_Texture.TryGet<Texture2D>(out var m_Texture2D)) //TODO other Texture
                    {
                        continue;
                    }

                    if (!Exported.Contains(m_Texture2D.m_Name))
                    {
                        var asset = new AssetItem(m_Texture2D);
                        asset.Text = m_Texture2D.m_Name;
                        Exporter.ExportTexture2D(asset, ExportFolder + "Texture2D/");
                        Exported.Add(m_Texture2D.m_Name);
                    }

                }

                MaterialList.Add(iMat);
            }
            else
            {
                iMat = new ImportedMaterial();
            }
            return iMat;
        }

        private static void SetFrame(ImportedFrame frame, Vector3 t, Quaternion q, Vector3 s)
        {
            frame.LocalPosition = new Vector3(-t.X, t.Y, t.Z);
            frame.LocalRotation = Fbx.QuaternionToEuler(new Quaternion(q.X, -q.Y, -q.Z, q.W));
            frame.LocalScale = s;
        }

        private ImportedFrame ConvertTransform(Transform trans)
        {
            var frame = new ImportedFrame(trans.m_Children.Length);
            transformDictionary.Add(trans, frame);
            trans.m_GameObject.TryGet(out var m_GameObject);
            frame.Name = m_GameObject.m_Name;
            SetFrame(frame, trans.m_LocalPosition, trans.m_LocalRotation, trans.m_LocalScale);
            return frame;
        }


        private void ConvertTransforms(Transform trans, ImportedFrame parent)
        {
            var frame = ConvertTransform(trans);
            if (parent == null)
            {
                RootFrame = frame;
            }
            else
            {
                parent.AddChild(frame);
            }
            foreach (var pptr in trans.m_Children)
            {
                if (pptr.TryGet(out var child))
                    ConvertTransforms(child, frame);
            }
        }
    }
}
