# BallCollect 実装計画（スライド6の3段階）

## 段階1 — シーン
- 床(20×20)・Player球(0, 0.5, −7)・アイテム5個(X=0, Z=−4,−1,2,5,8)・固定カメラ
- 既存のMain CameraとDirectional Lightを確認し、重複させない（既存カメラを固定視点として使用）
- 確認: Hierarchy再取得、Game Viewで配置、シーン保存

## 段階2 — 移動
- Input System（UnityEngine.InputSystem.Keyboard.current）でC#実装
- Playerへ接続、Rigidbodyで速度制御
- compile error 0を独立して確認 → シーン保存 → Play → W保持(1.0s)→解除→フレーム待ち → 前後のTransform取得（+Z増加を確認）→ Stop

## 段階3 — ルール
- B3 回収: Trigger Collider、触れたアイテム消去・+1・二重加算なし（アイテム名で一意管理）
- B4 クリア: 5個でCLEAR表示、途中では非表示。UIはCanvas+Text（Count / CLEAR）
- B5 落下: y<−10 で位置・速度・回収数・アイテム・UIを初期化（Start時の状態から復元）
- B6 R: Input Systemのキーで全初期化（途中・クリア後同一）

## 検証（スライド11）
- 状態の根拠: Transform座標、回収数、アイテムの有無（get_component_values / eval_csharp）
- 画面の根拠: Game画像 includeUI true（capture_screenshot, captureMode=game）
- verification.md に条件ごとに 操作・期待・実際・根拠・判定（成立/未成立/未確認）

## 追加機能（スライド12〜13、選定: 30秒タイムリミット A1〜A5）
- 優先順位: 5個目の回収と時間切れが同時に起きたら回収完了を優先
- 実30秒で確認。短縮して試した場合は短縮検証と記録し、最後に30秒へ戻す
