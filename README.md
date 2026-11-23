# Sky Meadow／晴空草原

Unity 6000.6.8f1 製作的第三人稱 3D 草原探索原型。原創角色 Pip 採卡通平台冒險家的比例，已套用提供的 Tripo 精細角色與金幣模型。

## 遊玩與開啟

- 從 GitHub 取得專案前先安裝 Git LFS，執行 `git lfs install`，再 `git clone https://github.com/Yee5772/ar-vr-3d-game.git`。若模型只有指標檔，進入專案後執行 `git lfs pull`。
- 儲存庫包含完整 Unity 原始專案與模型；`Library`、`Builds`、日誌及重複 ZIP 不提交。剛下載時請先在 Unity 開啟場景，或執行 `Tools/Build.ps1` 產生 Windows 版本，再使用 `Play.cmd`。

- 直接執行 `Builds/Windows/SkyMeadow.exe`，或雙擊 `Play.cmd`。
- 在 Unity Hub 選「Add project from disk」，選取本資料夾 `SkyMeadow`。
- 開啟 `Assets/Scenes/Grassland.unity`，按 Play。
- 第一次開啟若場景尚未存在，Editor 會自動生成。也可用選單 `Sky Meadow > Rebuild World` 重建；這會重設草原場景，請先保存自己的修改副本。

## 操作

| 按鍵 | 功能 |
|---|---|
| WASD／方向鍵 | 依鏡頭方向移動 |
| Space | 跳躍；空中再按一次二段跳 |
| 左 Shift | 奔跑 |
| E | 空中衝刺；落地後恢復 |
| 點擊畫面／按住滑鼠右鍵 | 旋轉鏡頭 |
| 滾輪 | 調整鏡頭距離 |
| Esc | 釋放滑鼠 |
| R | 返回最近的地面安全點 |
| Tab | 暫停／繼續 |

角色可以探索整片草原，沿浮空踏台跳向高處。跳躍有高度上限；二段跳和衝刺可跨越平台間隙。收集進度顯示在左上角，最高收集數保存在本機；重新開局會重置本局金幣。

## 地圖

- 240×240 公尺起伏草原、28,800 個地面三角形，MeshCollider 地面碰撞。
- 20 座固定浮空踏台，每階約上升 2.7 公尺；單次滿跳理論高度約 3.27 公尺。
- 35 枚地面路線金幣，39 枚天空路線金幣，總共 74 枚。
- 草原、踏台、樹木、岩石與雲由程式生成。場景保存後可在 Hierarchy 個別編輯。
- `Assets/Art/MapModels/Grassland.obj` 和 `.mtl` 是可攜出的原創草原網格，與遊戲地面使用相同高度公式；單位為公尺，Y 軸朝上。
- `Assets/Art/Generated/Grassland.asset` 是場景實際使用的 Unity 網格。

## 四方向模型參考圖

依你的最新要求，每個實體分開保存四張 PNG，沒有把四個方向拼成一張：

```text
Assets/Art/ModelReferences/
  Pip/
    前視圖.png
    左視圖.png
    右視圖.png
    後視圖.png
  WindCoin/
    前視圖.png
    左視圖.png
    右視圖.png
    後視圖.png
```

左右以角色自身方向定義；本參考的左視圖鼻子朝畫面左邊，右視圖鼻子朝畫面右邊。圖片為 AI 生成的建模參考，小配件與輪廓可能有細微差異，建模時以前視圖定比例、以側視圖定厚度。金幣前後採相同風星圖案，左右邊緣造型相同。

原始三方向拼圖保留在 `Design/Archive` 作為歷史資料；正式建模請使用上面的四張獨立圖片。生成方式與提示詞見 `Design/ImagePrompts.md`。

## 替換精細模型

目前已解壓並套用的模型位於 `Assets/Art/RefinedModels/Pip/Source` 和 `Assets/Art/RefinedModels/WindCoin/Source`。原始兩份 ZIP 僅保留本機；Git 儲存庫包含已解壓的 FBX 與貼圖。角色腳底已對齊地面、身高 2.2 公尺；金幣直徑 1 公尺，使用原有收集邏輯。角色保留 Ignore Raycast 圖層及 CharacterController。

原始 FBX 沒有骨架或動畫；專案現在會為 Pip 建立 16 個骨骼與四權重蒙皮，依角色實際狀態播放待機、走路、奔跑、跳起、下落、落地及衝刺的程式化動作。生成的蒙皮網格是 `Pip/PipSkinned0.asset`，動作邏輯為 `MeadowCharacterAnimation`。原始高精度 FBX 保留，後續仍可提供專業綁骨與動畫版本替換。

原始角色約 199 萬、金幣約 198 萬個三角形；遊戲使用的減面版本分別為 168,780 與 218,506 個三角形，另啟用 GPU 蒙皮。減面快取位於 `Assets/Art/Optimized`，金幣透過 LODGroup 在遠處改用簡易造型。浮空地塊及實心圓柱採貼合模型的 MeshCollider，避免圓柱預設膠囊碰撞體產生凸起。暫停時角色不再捕捉滑鼠，Resume 與 Quit game 可使用滑鼠操作。

離線減面使用 MIT 授權的 [UnityMeshSimplifier](https://github.com/Whinarn/UnityMeshSimplifier)，編輯器原始碼、版本與授權位於 `Assets/Editor/ThirdParty/UnityMeshSimplifier`，不納入遊戲執行時程式。

`Sky Meadow > Apply Refined Models` 可重新套用這兩份 FBX，並更新目前場景的角色與金幣實例。此動作會替換這兩類物件的外觀；先備份自行調整過的版本。正背面檢查圖在 `Design/ModelPreviews`。批次更新與打包入口為 `SkyMeadow.Editor.RefinedModelInstaller.BuildInstalled`，沿用既有草原場景。

1. 將角色 FBX／GLB 等 Unity 支援的模型及貼圖放入 `Assets/Art/RefinedModels/Pip`；金幣放入 `Assets/Art/RefinedModels/WindCoin`。
2. 開啟 `Assets/Prefabs/Pip.prefab`。保留根物件的 `CharacterController` 和 `MeadowPlayer`，只替換 `Visual` 子物件的幾何。角色腳底位於 local Y=0、面朝 +Z，總高建議約 2.2 公尺。
3. 將 `MeadowPlayer.visual` 指向新的模型根節點；模型所有子物件 Layer 設為 Ignore Raycast，避免鏡頭撞上角色。
4. 金幣保留 `WindCoin` 根腳本，替換其子模型，直徑約 1 公尺，厚約 0.15 公尺；中心在 local 原點、正面朝 +Z。
5. 場景中的實體已連結 Prefab，套用 Prefab 修改即可同步。重建世界會重新產生簡易外觀；選 Apply Refined Models 可重新套用目前 FBX，BuildAll 也會在重建後套用。

## 開發與驗證

`Assets/Scripts`：角色、鏡頭、金幣、HUD 與世界生成。

`Assets/Editor/ProjectBuilder.cs`：保存場景、Prefab、材質與網格，建立 Windows 版本。`Tools/Build.ps1` 可批次執行，需要已啟用的 Unity 授權。

`Tools/CompileCheck.ps1`：使用已安裝 Unity 的 Roslyn 與引擎 DLL 進行 C# 編譯檢查。`Tools/GenerateMap.py` 可重新輸出 OBJ。

`Validation.txt` 是場景結構檢查。執行 `SkyMeadow.exe -smokeTest` 會檢查落地、拾取、重生、20 個平台中心與邊緣的表面高度、平台邊緣實際落地、蒙皮肢體變形及暫停／繼續，輸出 `RuntimeValidation.txt`、`Gameplay.png` 後離開。這些檢查不代表整張地圖已完成人工遊玩測試。

目前為探索原型：已接上精細模型與基本骨架動作，尚無敵人、任務、完整音樂或大型世界串流。後續可加入更細緻的手工動畫，以及更適合即時遊戲的減面模型。

角色碰撞與移動採 Unity CharacterController，重力在角色腳本中處理。官方 API：[CharacterController.Move](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/charactercontroller/move)。
