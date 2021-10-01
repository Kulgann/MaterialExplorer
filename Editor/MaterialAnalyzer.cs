using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Rendering;

public class MaterialAnalyzer : EditorWindow
{
    private static Dictionary<Material, AnalyzerData> materialDicts;
    private static List<AnalyzerData> analyzerList;
    public CustomColorPropDef customColorProps;
    Vector2 scrollPos;
    bool[] showGameObjects;
    int boolBufferPos = 0;
    Rect windowVisibleRect;
    private GUIStyle matBoxStyle;
    private RenderPipeType renderPipeType;
    private Color tintColor;
    MultiColumnHeader columnHeader;
    MultiColumnHeaderState.Column[] columns;


    [MenuItem("Window/Rendering/Material Explorer",false,3)]



    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MaterialAnalyzer));
      

    }

    void OnEnable()
    {
        matBoxStyle = new GUIStyle(EditorStyles.helpBox);
        matBoxStyle.margin = new RectOffset(0, 0, 10, 10);
        EditorApplication.hierarchyChanged += OnHierarchyChange;
        FindMaterials();
        DetectRenderPipe();
        InitColumns();
       // LoadCustomPropColors();
    }

    void LoadCustomPropColors()
    {
        string[] guid = AssetDatabase.FindAssets("t:" + typeof(CustomColorPropDef).Name);
        string path = AssetDatabase.GUIDToAssetPath(guid[0]);
        customColorProps = AssetDatabase.LoadAssetAtPath<CustomColorPropDef>(path);
    }

    void InitColumns()
    {
        columns = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Materials"),
                    width = 100,
                    minWidth = 100,
                    maxWidth = 500,
                    autoResize = true,
                    headerTextAlignment = TextAlignment.Center
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Shader"),
                    width = 100,
                    minWidth = 100,
                    maxWidth = 500,
                    autoResize = true,
                    headerTextAlignment = TextAlignment.Center
                },
                 new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("GameObjects"),
                    width = 100,
                    minWidth = 100,
                    maxWidth = 500,
                    autoResize = true,
                    headerTextAlignment = TextAlignment.Center
                },
                  new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("TEST"),
                    width = 500,
                    minWidth = 100,
                    maxWidth = 500,
                    autoResize = true,
                    headerTextAlignment = TextAlignment.Center
                },
                  new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("TEST2"),
                    width = 500,
                    minWidth = 100,
                    maxWidth = 500,
                    autoResize = true,
                    headerTextAlignment = TextAlignment.Center
                },
            };
        columnHeader = new MultiColumnHeader(new MultiColumnHeaderState(columns));
        columnHeader.height = 25;
        columnHeader.ResizeToFit();
    }

    void DetectRenderPipe()
    {
        if (GraphicsSettings.currentRenderPipeline)
        {
            if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
            {
                renderPipeType = RenderPipeType.HighDefinition;
            }
            else 
            {
                renderPipeType = RenderPipeType.URP;
            }
        }
        else
        {
            renderPipeType = RenderPipeType.Default;
        }
    }

    string SetColorProperty(Material mat)
    {

        switch (renderPipeType)
        {
            case RenderPipeType.URP:
            case RenderPipeType.HighDefinition:
                if(mat.HasProperty("_BaseColor"))
                {
                    return "_BaseColor";
                }
                else
                {
                    //Go trough the custom list of Color prop names
                    for (int i = 0; i < customColorProps.CustomColorPropDefines.Count; i++)
                    {
                        if(mat.HasProperty(customColorProps.CustomColorPropDefines[i]))
                        {
                            return customColorProps.CustomColorPropDefines[i];
                        }
                    }
                }
                break;
            case RenderPipeType.Default:
                if(mat.HasProperty("_Color"))
                {
                    return ("_Color");
                }
                else
                {
                    //Go trough the custom list of Color prop names
                    for (int i = 0; i < customColorProps.CustomColorPropDefines.Count; i++)
                    {
                        if (mat.HasProperty(customColorProps.CustomColorPropDefines[i]))
                        {
                            return customColorProps.CustomColorPropDefines[i];
                        }
                    }
                }
                break;
        }

        return "";
    }


    void OnGUI()
    {
        #region old_stuff
        ////GUILayout.FlexibleSpace();



        //windowVisibleRect.width = position.width;
        //windowVisibleRect.height = position.height;
        //// draw the column headers
        //var headerRect = windowVisibleRect;
        //headerRect.height = columnHeader.height;

        //float xScroll = scrollPos.x;

        //columnHeader.OnGUI(headerRect, xScroll);


        //var contentRect = columnHeader.GetColumnRect(0);
        //contentRect.x -= xScroll;
        //contentRect.y = contentRect.yMax;
        //contentRect.yMax = windowVisibleRect.yMax;
        //Rect rowRect = new Rect(contentRect.x, contentRect.y, windowVisibleRect.width, 20);

        //GUI.DrawTexture(contentRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f, new Color(0f, 0f, 1f, 0.5f), 4, 10);
        //for (int j = 0; j < analyzerList.Count; j++)
        //{

        //    Rect MatcellRect = columnHeader.GetCellRect(0, rowRect);
        //    Rect ShaderCellRect = columnHeader.GetCellRect(1, rowRect);
        //    Rect GameObjectsRect = columnHeader.GetCellRect(2, rowRect);

        //    ShaderCellRect.xMin += 5f;
        //    GameObjectsRect.xMin += 5f;
        //    rowRect.y += 25;
        //     EditorGUI.ObjectField(MatcellRect, GUIContent.none, analyzerList[j].mat, typeof(Material), false);
        //     EditorGUI.ObjectField(ShaderCellRect, GUIContent.none, analyzerList[j].shader, typeof(Shader), false);

        //    analyzerList[j].ShowGameObject = EditorGUI.Foldout(GameObjectsRect, analyzerList[j].ShowGameObject, "Show " + analyzerList[j].gameObjects.Count + " GameObjects");
        //    if (analyzerList[j].ShowGameObject)
        //    {
        //        rowRect.height = GameObjectsRect.height * analyzerList[j].gameObjects.Count;
        //        foreach (GameObject go in analyzerList[j].gameObjects)
        //        {
        //            GameObjectsRect.y += GameObjectsRect.height;
        //            EditorGUI.ObjectField(GameObjectsRect, GUIContent.none, go, typeof(Material), false);
        //           // GUI.DrawTexture(GameObjectsRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f, new Color(0f, 0f, 1f, 0.5f), 4, 10);


        //        }
        //    }
        //   // GUI.DrawTexture(rowRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f, new Color(0f, 1f, 0f, 0.5f), 4, 10);
        //}


        // draw the column's contents
        //for (int i = 0; i < columns.Length; i++)
        //{
        //    // calculate column content rect
        //    var contentRect = columnHeader.GetColumnRect(i);
        //    contentRect.x -= xScroll;
        //    contentRect.y = contentRect.yMax;
        //    contentRect.yMax = windowVisibleRect.yMax;
        //    Rect rowRect = new Rect(contentRect.x, contentRect.y, windowVisibleRect.width,20);
        //    GUI.DrawTexture(contentRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f, new Color(1f, 0f, 0f, 0.5f), 4, 10);
        //    GUI.DrawTexture(rowRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f, new Color(0f, 1f, 0f, 0.5f), 4, 10);
        //    for (int j = 0; j < analyzerList.Count; j++)
        //    {

        //        Rect MatcellRect = columnHeader.GetCellRect(0, rowRect);
        //        Rect ShaderCellRect = columnHeader.GetCellRect(1, rowRect);
        //        Rect GameObjectsRect = columnHeader.GetCellRect(2, rowRect);

        //        ShaderCellRect.xMin += 5f;
        //        GameObjectsRect.xMin += 5f;
        //        rowRect.y += 25;
        //       // EditorGUI.ObjectField(MatcellRect, GUIContent.none, analyzerList[j].mat, typeof(Material), false);
        //       // EditorGUI.ObjectField(ShaderCellRect, GUIContent.none, analyzerList[j].shader, typeof(Shader), false);

        //        analyzerList[j].ShowGameObject = EditorGUI.Foldout(GameObjectsRect,analyzerList[j].ShowGameObject, "Show " + analyzerList[j].gameObjects.Count + " GameObjects");
        //        if (analyzerList[j].ShowGameObject)
        //        {
        //            foreach (GameObject go in analyzerList[j].gameObjects)
        //            {
        //                GameObjectsRect.y += GameObjectsRect.height;
        //              //  EditorGUI.ObjectField(GameObjectsRect, GUIContent.none, go, typeof(Material), false);

        //                rowRect.y += GameObjectsRect.height;
        //            }
        //        }

        //    }



        // custom content GUI...

        //}

        //windowVisibleRect = GUILayoutUtility.GetLastRect();
        #endregion


        Material temp = null;
        if (Event.current.commandName == "ObjectSelectorUpdated")
            Selection.activeObject = temp;
        if (analyzerList.Count > 0)
        {
          
            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos, true, true);

            foreach (AnalyzerData data in analyzerList)
            {
                
                GUILayout.BeginHorizontal(matBoxStyle);
                Rect previewTextureRect = GUILayoutUtility.GetRect(128, 128, 128, 128, GUILayout.Width(128));
               
                GUI.DrawTexture(previewTextureRect, AssetPreview.GetAssetPreview(data.mat));

                temp = (Material)EditorGUILayout.ObjectField(data.mat, typeof(Material), false, GUILayout.Width(200));

                string colorProp = SetColorProperty(data.mat);

                if (colorProp != "")
                {

                    
                    EditorGUI.BeginChangeCheck();
                    tintColor = EditorGUILayout.ColorField(data.mat.GetColor(colorProp), GUILayout.Width(100));
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(data.mat, "Base color change on " + data.mat.name);
                        data.mat.SetColor(colorProp, tintColor);
                    }
                 }
                //if (data.shaderKeywords.Length > 0)
                //{
                    string keywords = "";
                    foreach (string keyword in data.shaderKeywords)
                    {
                        keywords += " " + keyword;
                    }

                    GUILayout.TextArea(keywords, GUILayout.Width(500));
                    
                //}
                EditorGUILayout.ObjectField(data.shader, typeof(Shader), false, GUILayout.MaxWidth(300),GUILayout.MinWidth(100));
              
                GUILayout.BeginVertical();
               
                data.ShowGameObject = EditorGUILayout.Foldout(data.ShowGameObject, "Show " + data.gameObjects.Count + " GameObjects", EditorStyles.foldout);
                if (data.ShowGameObject)
                {
                    foreach (GameObject go in data.gameObjects)
                    {
                        EditorGUILayout.ObjectField(go, typeof(GameObject), false, GUILayout.MaxWidth(200));
                    }
                }
               
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                EditorGUI.EndChangeCheck();
            }
           

            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            EditorGUI.EndChangeCheck();
        }
    }

    void OnHierarchyChange()
    {
        FindMaterials();
    }

        private void FindMaterials()
        {

        materialDicts = new Dictionary<Material, AnalyzerData>();
        analyzerList = new List<AnalyzerData>();
        GameObject[] allObject = FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObject)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                Material[] currentObjectMts = rend.sharedMaterials;
                foreach (Material mat in currentObjectMts)
                {
                    if (mat != null)
                    {
                        if (!materialDicts.ContainsKey(mat))
                        {
                            AnalyzerData data = new AnalyzerData(mat.name, mat, go, mat.shaderKeywords, mat.shader);
                            materialDicts.Add(mat, data);
                        }
                        else
                        {
                            materialDicts[mat].gameObjects.Add(go);
                        }
                    }
                }
            }
        }
        //Sort the dict alphabetically
        foreach (KeyValuePair<Material, AnalyzerData> keyValuePair in materialDicts)
        {
            analyzerList.Add(keyValuePair.Value);
        }
        analyzerList = analyzerList.OrderBy(x => x.matName).ToList();
    }

}

public class AnalyzerData
{
    public string matName;
    public Material mat;
    public List<GameObject> gameObjects;
    public string[] shaderKeywords;
    public Shader shader;
    public bool ShowGameObject;

    public AnalyzerData(string matName,Material mat, GameObject gameObject, string[] shaderKeywords, Shader shader)
    {
        this.matName = matName;
        this.mat = mat;
        this.gameObjects = new List<GameObject>();
        this.gameObjects.Add(gameObject);
        this.shaderKeywords = shaderKeywords;
        this.shader = shader;
        ShowGameObject = false;
    }
}

public enum RenderPipeType
{
    HighDefinition,
    URP,
    Default
}
