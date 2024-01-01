using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://juejin.cn/post/7110013262606893069
public class FPSDisplay : MonoBehaviour
{
    float updateInterval = 0.5f;           //当前时间间隔
    private float accumulated = 0.0f;      //在此期间累积  
    private float frames = 0;              //在间隔内绘制的帧  
    private float timeRemaining;           //当前间隔的剩余时间
    private float fps = 0f;                //当前帧 Current FPS
    private float lastSample;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject); //不销毁此游戏对象，在哪个场景都可以显示，，不需要则注释
        timeRemaining = updateInterval;
        lastSample = Time.realtimeSinceStartup; //实时自启动
    }
    
    void Update()
    {
        ++frames;
        float newSample = Time.realtimeSinceStartup;
        float deltaTime = newSample - lastSample;
        lastSample = newSample;
        timeRemaining -= deltaTime;
        accumulated += 1.0f / deltaTime;
  
        if (timeRemaining <= 0.0f)
        {
            fps = accumulated / frames;
            timeRemaining = updateInterval;
            accumulated = 0.0f;
            frames = 0;
        }
    }

    void OnGUI()
    {
        float o = 50 * (Screen.height / 1080f);
        var style = new GUIStyle
        {
            border = new RectOffset(10, 10, 10, 10),
            fontSize = (int)o,
            fontStyle = FontStyle.Bold,            
        };
        //自定义宽度 ，高度大小 颜色，style
        GUI.Label(new Rect(o, o, 200, 200), $"{fps:f2}", style);
    }
}
