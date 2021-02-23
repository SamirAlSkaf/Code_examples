using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CaveMesh : MonoBehaviour
{
    private static Object[] AllTextures;
    public int TextureWidth = 4000;
    public int TextureHeight = 1000;
    public int MinTexturePartWidth = 800;
    public int MaxTexturePartWidth = 1600;
    public int FadeWidth = 400;

    private Texture2D[] composeRandomPart(int width)
    {
        // create texture part slices
        Texture2D[] texturePartSlices = { new Texture2D(FadeWidth, TextureHeight), new Texture2D(width - 2 * FadeWidth, TextureHeight), new Texture2D(FadeWidth, TextureHeight) };

        // pick a random texture and a random color
        Texture2D randomTexture = (Texture2D) AllTextures[Random.Range(0, AllTextures.Length)];
        Color randomColor = new Color(Random.value, Random.value, Random.value, Random.value * .6f + .2f);
        Texture2D randomColoredTexture = new Texture2D(width, TextureHeight);

        // mix texture and color and put it in the complete texture
        for (int y = 0; y < randomTexture.height; y++)
        {
            for (int x = 0; x < randomTexture.width; x++)
            {
                Color textureColor = randomTexture.GetPixel(x % randomTexture.width, y % randomTexture.height);
                Color newColor = new Color(randomColor.r * randomColor.a + textureColor.r * (1 - randomColor.a), randomColor.g * randomColor.a + textureColor.g * (1 - randomColor.a), randomColor.b * randomColor.a + textureColor.b * (1 - randomColor.a), 1);
                randomColoredTexture.SetPixel(x, y, newColor);
            }
        }

        // fill complete texture vertically
        int composedHeight = randomTexture.height;
        while (composedHeight < TextureHeight)
        {
            int addedHeight = (TextureHeight - composedHeight > randomTexture.height) ? randomTexture.height : (TextureHeight - composedHeight);
            randomColoredTexture.SetPixels(0, composedHeight, randomTexture.width % width, addedHeight, randomColoredTexture.GetPixels(0, 0, randomTexture.width % width, addedHeight));
            composedHeight += addedHeight;
        }

        // fill complete texture horizontally
        int composedWidth = randomTexture.width;
        while (composedWidth < width)
        {
            int addedWidth = (width - composedWidth > randomTexture.width) ? randomTexture.width : width - composedWidth;
            randomColoredTexture.SetPixels(composedWidth, 0, addedWidth, TextureHeight, randomColoredTexture.GetPixels(0, 0, addedWidth, TextureHeight));
            composedWidth += addedWidth;
        }

        // divide complete texture into 3
        texturePartSlices[0].SetPixels(0, 0, FadeWidth, TextureHeight, randomColoredTexture.GetPixels(0, 0, FadeWidth, TextureHeight));
        texturePartSlices[1].SetPixels(0, 0, width - 2 * FadeWidth, TextureHeight, randomColoredTexture.GetPixels(FadeWidth, 0, width - 2 * FadeWidth, TextureHeight));
        texturePartSlices[2].SetPixels(0, 0, FadeWidth, TextureHeight, randomColoredTexture.GetPixels(width - FadeWidth, 0, FadeWidth, TextureHeight));
        
        texturePartSlices[0].Apply();
        texturePartSlices[1].Apply();
        texturePartSlices[2].Apply();
        Destroy(randomColoredTexture);
        return texturePartSlices;
    }

    public Texture2D fadeTextureFromTo(Texture2D fromTexture, Texture2D toTexture, int width)
    {
        Texture2D fadedTexture = new Texture2D(width, TextureHeight);
        float value = 1;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < TextureHeight; y++)
            {
                Color fromColor = fromTexture.GetPixel(fromTexture.width - width + x, y);
                Color toColor = toTexture.GetPixel(x, y);
                Color newColor = new Color((fromColor.r * value + toColor.r * (1f - value)), (fromColor.g * value + toColor.g * (1f - value)), (fromColor.b * value + toColor.b * (1f - value)));
                fadedTexture.SetPixel(x, y, newColor);
            }
            value -= 1 / (float)width;
        }

        fadedTexture.Apply();
        return fadedTexture;
    }

    public void assignRandomTexture()
    {
        Texture2D composedTexture = new Texture2D(TextureWidth, TextureHeight);

        List<Texture2D[]> randomTextureList = new List<Texture2D[]>();

        int x = 0;
        bool isLastPart = false;
        while (x < TextureWidth)
        {
            int randomWidth;
            if (TextureWidth - x > MinTexturePartWidth + MaxTexturePartWidth - 2 * FadeWidth)
            {
                // generate textureparts
                randomWidth = Random.Range(MinTexturePartWidth, MaxTexturePartWidth);
            }
            else
            {
                // generate last 2 textureparts
                if (isLastPart == false)
                {
                    isLastPart = true;
                    randomWidth = Random.Range(MinTexturePartWidth, TextureWidth - x - MinTexturePartWidth + 2 * FadeWidth);
                }
                else 
                {
                    randomWidth = TextureWidth - x + FadeWidth;
                }
            }
            randomTextureList.Add(composeRandomPart(randomWidth));
            x += randomWidth - FadeWidth;
        }

        Texture2D fadedTexture = null;

        x = 0;
        for (int i = 0; i < randomTextureList.Count; i++)
        {
            if (i == randomTextureList.Count - 1)
            {
                composedTexture.SetPixels(x, 0, randomTextureList[i][1].width, TextureHeight, randomTextureList[i][1].GetPixels(0, 0, randomTextureList[i][1].width, TextureHeight));
                x += randomTextureList[i][1].width;
                fadedTexture = fadeTextureFromTo(randomTextureList[i][2], randomTextureList[0][0], FadeWidth);
                composedTexture.SetPixels(x, 0, randomTextureList[i][2].width, TextureHeight, fadedTexture.GetPixels());
                Destroy(fadedTexture);
                x += randomTextureList[i][2].width;
            }
            else
            {
                composedTexture.SetPixels(x, 0, randomTextureList[i][1].width, TextureHeight, randomTextureList[i][1].GetPixels(0, 0, randomTextureList[i][1].width, TextureHeight));
                x += randomTextureList[i][1].width;
                fadedTexture = fadeTextureFromTo(randomTextureList[i][2], randomTextureList[i+1][0], FadeWidth);
                composedTexture.SetPixels(x, 0, randomTextureList[i][2].width, TextureHeight, fadedTexture.GetPixels());
                Destroy(fadedTexture);
                x += randomTextureList[i][2].width;
            }
        }

        foreach (var tex in randomTextureList.SelectMany(tex => tex).ToArray())
        {
            Destroy(tex);
        }
        Destroy(GetComponent<Renderer>().material.mainTexture);

        GetComponent<Renderer>().material.mainTexture = composedTexture;
        composedTexture.Apply();
    }

    public void Start()
    {
        AllTextures = Resources.LoadAll("RandomTextures", typeof(Texture2D));
        //assignRandomTexture();
    }
}
