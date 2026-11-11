using UnityEditor;

namespace HollowSpirit.Editor
{
    /// <summary>Imports the skeleton model and its clips from the git-ignored DungeonCharacters pack as Humanoid, with the model at enemy height.</summary>
    public sealed class SkeletonImportPostprocessor : AssetPostprocessor
    {
        private const string PackFolder = "Assets/DungeonCharacters/Skeletons_demo/";
        private const string ModelPath = PackFolder + "models/DungeonSkeleton_demo.FBX";

        // Idle height of the pack model is about 2.31 m; 0.82 brings it to about 1.9 m.
        private const float ModelScale = 0.82f;

        private static readonly string[] ClipPaths =
        {
            PackFolder + "animation/DS_onehand_idle_A.FBX",
            PackFolder + "animation/DS_onehand_walk.FBX",
            PackFolder + "animation/DS_onehand_attack_A.FBX",
        };

        /// <summary>Bump when the settings below change so Unity reimports the pack files.</summary>
        public override uint GetVersion()
        {
            return 2;
        }

        private void OnPreprocessModel()
        {
            bool isModel = assetPath == ModelPath;
            if (!isModel && System.Array.IndexOf(ClipPaths, assetPath) < 0)
            {
                return;
            }

            ModelImporter importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            if (isModel)
            {
                importer.globalScale = ModelScale;
            }
        }
    }
}
