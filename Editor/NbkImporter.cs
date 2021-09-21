using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace Nabuki.Editor
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "tsv")]
    public class TSVImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            TextAsset subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }

    [UnityEditor.AssetImporters.ScriptedImporter(1, "nbk")]
    public class NbkImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            TextAsset subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}