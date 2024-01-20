# Dancing Line Fanmade Template

__本项目为跳舞的线自制关卡模板，还在更新中，非最终版本，请选择性使用！__

__请注意，使用本模板需要有一定的 C# 基础，不提供 C# 基础教程！__

如果您没有任何 C# 基础，您可以学习一下 C# 或者也可以在[这里](https://chinadlrs.com/developer/)选择其他模板使用

## 环境要求

- Unity 2019.3.10f1 及以上

## 可选的编辑使用的 文本编辑器 / IDE

- [Visual Studio Code](https://code.visualstudio.com/)
  - 功能丰富的文本编辑器，虽然也可以用但是个人不太推荐拿来写 C#
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
  - 体验还行，但是配合 Unity 的使用体验上可能没有 Rider 舒服
- [Rider](https://www.jetbrains.com/rider/)
  - 个人推荐，配合 Unity 使用体验上很舒服，~~但是付费~~

## 一些吐槽

太喜欢写 Manager (Singleton) 了导致了到处都是 Instance 跳来跳去的不太好拓展见谅，如果需要自己拓展一些功能可能需要修改一下模板的代码

## 更新日志

### 2024/1/1 - 上传到 GitHub
- 上传到 GitHub

### 2024/1/1 - 2024010100 更新
- 修改了部分类的访问权限到 `internal`
- 将 `ReadyUI` 作为单独类便于管理
- 新增暂停界面 UI 便于返回起始状态
- 优化了 UIManager.cs 的结构，将各种 UI 整合到 UIComponents.cs 里了

### 2024/1/2 - 2024010200 更新
- 现在会在 `MainLine` 加载实例时 (`Awake`) 自动设置 `_onGround`，避免会在初次开始游玩时有粒子产生 
- 增加空场景 (此空场景为 SampleScene 修改，并非重新制作) 便于制作

### 2024/1/4 - 2024010400 更新
> 不确定修改为抛物线是否会对性能产生较大的影响，可以选择改回 CubicOut + CubicIn 的形式
- 修改 `ButtonAttribute` 的命名空间到 `DancingLineSample.Attributes` 下
- 修改皇冠效果动画为抛物线 (CheckpointObject.cs)
- 新增皇冠效果动画抛物线预览
- 新增相机跟随数据预览
- 修复打包时报错的问题 (为 UnityEditor 相关引起的问题)
- 增加 AudioOffset 按钮组 (`UICanvas/Setting/View/Top/AudioOffset`)，为下次更新准备

### 2024/1/4 - 2024010401 更新
> 与 `2024010400 更新` 相同
- 移除不该存在的 packages.config

### 2024/1/6 - 2024010600 更新
> 因音音频延迟调整系统的加入，修改了 `GameplayManager.Play(float)` 方法，原方法内容移动到 `GameplayManager.PlayTask(float)` 内并修改

> ### 关于音频延迟调整系统说明  
> 当你感觉 LATE 较多时 (线过晚) 可以增加延迟  
> 当你感觉 EARLY 较多 (线过早) 时可以减少延迟
> 
> 未来会加入让玩家自己测延迟的功能 (延迟向导)

- 移除了 `MathUtility.FitParabola(float, float, float)` 方法对于 `MathNet` 的依赖，性能或许会好些
- 新增音频延迟调整系统 (和 `GameplayManager.AudioOffset` 不同，该值是玩家可以更改的)
- 修复皇冠动画效果不正确的问题
- 微调了默认皇冠动画效果的高度，如需调整可自行调整
- 空场景将不再更新，待模板完善后更新，主要还是~~懒得更~~

### 2024/1/7 - 2024010700 更新
> `UIManager` 可能会在未来某个更新中重构
- 重做了 UI_Retry 的 svg
- 新增偏移向导 UI （`OffsetWizardUI`）
- 将所有的 UI 组件修改为以 `UIComponent` 为基类方便管理
- 新增延迟校准向导
- 修改 `ButtonAttribute` 的名称为 `MethodButtonAttribute`，避免与 `UnityEngine.UI` 中的 `Button` 冲突
- 新增 DSP Buffer 设置
- 修改 `AudioOffsetManager` 的名称为 `AudioManager` 便于管理音频相关设置
- 修复了 `AudioManager.AudioOffset < 0` 时等待期间可以一直点击屏幕播放音频的问题
- 修复了 `InfoDisplay` 自适应不正确的问题

### 2024/1/18 - 2024011800 更新
- 修复了部分值按钮组的值不会被正确设置的问题
- 新增开关 UI 模糊功能
- 新增了全屏 / 非全屏切换 (仅 `UNITY_STANDALONE_WIN`)
- 现在开始会在结算的时候保存存档了
- 现在开始默认会在打包后运行时的 `Awake` 自动销毁场景中的 `[Graphy]` 和 `IngameDebugConsole`

### 2024/1/20 - 2024012000 更新
- 在获取进度的时候增加了一个判断 Clip 是否为空的逻辑, 避免 `NullReferenceException`
- 修改了部分模板资源的位置
- 将关卡内容修改为 Prefab 便于直接使用 (`LevelContainer`)
- 空场景更新 (`LevelContainer` 测试)