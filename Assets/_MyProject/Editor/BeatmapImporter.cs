using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(version: 1, ext: "ugc")]
public class BeatmapImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        string text = File.ReadAllText(ctx.assetPath);
        var textAsset = new TextAsset(text)
        {
            name = Path.GetFileNameWithoutExtension(ctx.assetPath)
        };

        ctx.AddObjectToAsset("text", textAsset);
        ctx.SetMainObject(textAsset);
    }
}
