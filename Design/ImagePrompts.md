# 模型參考圖生成記錄

使用內建 image_gen 工具生成，沒有使用 CLI。正式交付為 `Assets/Art/ModelReferences` 中兩個實體各四張獨立 PNG。

## 角色原始設計提示詞

Use case: stylized-concept. Asset type: 3D game character orthographic modeling turnaround. Create one clean production reference sheet with exactly three aligned full-body views FRONT, RIGHT SIDE, BACK, same scale, neutral A-pose, white background, no perspective. Original cheerful chibi grassland explorer named Pip, inspired by classic colorful 3D platformer proportions: large head, round nose, expressive eyebrows, no mustache; teal aviator cap with small cream feather, orange short jacket, cream shirt, navy shorts, brown ankle boots, cream gloves, small teal backpack. Soft rounded stylized 3D render, clear clothing construction, consistent anatomy and accessories across views. No logos, no weapons, no additional props. Arms separated from torso. Crisp professional character model sheet, landscape layout. Only text FRONT RIGHT BACK.

## 金幣原始設計提示詞

Use case: stylized-concept. Asset type: orthographic 3D collectible game coin model sheet. Exactly three aligned views labeled FRONT, SIDE, BACK on a white landscape sheet. Same single golden coin, thick beveled edge, warm polished gold, embossed simple four-point wind-star on front and back, no letters or logos. Front and back circular, right side thin rounded rectangle showing 0.15 thickness relative to diameter 1.0. Soft studio light, crisp stylized 3D render, clear shape for 3D reconstruction. Coin alone, no pedestal, no character, no perspective, consistent proportions, generous spacing.

## 角色四方向提示詞

以下每個方向分別執行一次，參考圖為 `Design/Archive/Pip_ThreeViews.png`，共四次工具呼叫。將 `<VIEW>` 依序替換為下列文字：

- `front view, facing directly toward the camera, both eyes visible, symmetrical`
- `strict LEFT profile of the character, camera sees the character's anatomical LEFT side, nose points to the LEFT edge of the image`
- `strict RIGHT profile of the character, camera sees the character's anatomical RIGHT side, nose points to the RIGHT edge of the image`
- `strict back view, facing directly away from camera, backpack fully visible`

Use case: identity-preserve. Edit the reference model sheet into ONE standalone full-body orthographic <VIEW>. Preserve this exact Pip explorer design, clothing, colors, facial proportions, teal cap and goggles and feather, orange jacket, navy shorts, cream gloves, brown boots, teal backpack. Neutral A pose consistent with the sheet. White background, soft studio lighting, centered with complete head and boots visible and generous margin, square image. Exactly ONE character and ONE view, no turnarounds, no labels, no text, no dimension marks, no other figures. This is a precise 3D modeling reference.

## 金幣四方向提示詞

以下每個方向分別執行一次，參考圖為 `Design/Archive/WindCoin_ThreeViews.png`，共四次工具呼叫。將 `<VIEW>` 依序替換為下列文字：

- `FRONT face-on view of the circular front, four-point embossed wind star`
- `LEFT strict side profile, camera sees anatomical left edge of the upright coin, a thin vertical rounded rectangle, thickness .15 of its height, NO visible front or back face`
- `RIGHT strict side profile, camera sees the right edge of the upright coin, a thin vertical rounded rectangle, thickness .15 of its height, NO visible front or back face`
- `BACK face-on view of the circular reverse face, four-point embossed wind star`

Use case: identity-preserve. Convert reference sheet to ONE isolated orthographic <VIEW>. Preserve exact single gold collectible coin design, smooth beveled edge and warm gold. Centered on white square background, complete coin visible with margins, studio lighting. ONE coin ONE view only, no text, no labels, no dimension lines, no pedestal, no multiple views. This is a 3D modeling reference.
