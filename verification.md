# BallCollect — 検証結果 (verification)

実行環境: Unity Editor 6000.6.4f1 (headless), unity-cli 0.19.1 bridge via CLI. すべて実ツール出力。

## Stage 1 — シーン構築 (deck p.6 ①)
- 階層: Floor(plane, scale 2,2,2=20×20) / Player(sphere, tag Player, Rigidbody) / Item_1..5(tag Item, trigger, scale 0.4) / Main Camera(固定) ✓ (get_hierarchy)
- 配置: Player (0, 0.5, -7); Items X=0, Z=-4/-1/2/5/8; Camera position (0,10,-14), rotation (45,0,0) — 俯瞰固定カメラ ✓
- トリガー設定監査で Stage 1 の誤設定を検出・修正: Player SphereCollider isTrigger=True→False(物理)、Item_1..5 False→True。`set_component_field` は `m_IsTrigger` で成功(`isTrigger` は失敗)。再監査結果: Player=False, Items=True ✓ save_scene True

## Stage 2 — 移動 (deck p.6 ②)
- Input System パッケージ確認(manifest 参照) ✓。`create_csharp_file` は GitHub 403(LSP マニフェスト)で失敗 → evidence/stage2_create.json・stage2_compile.json(errorCount 0)。write_file で直接作成し refresh→errorCount 0、add_component 成功(find_by_component totalFound 1)
- Play 検証: 静止位置 (0.00, 0.50, -7.00)(床上の正しい静止) → W 1.0s press + release → z=-4.00(=speed 3×1s、+Z) ✓ WASD は dir=(x,0,z): W=+Z/S=-Z/D=+X/A=-X

## Stage 3 — ルール (deck p.6 ③)
最終 clean Play の結果(修正後):
- baseline: count=0 ✓ (B1)
- W walk で Item_1..5 を順に接触 → 各 item SetActive(false)+count+1、count=5 ✓ (B3)
- 二重カウント防止: 取得済み Item_1 の再通過で count 増えず ✓ (collected HashSet)
- CLEAR 判定: count==5 のときのみ clearShown=True ✓ (B4; OnGUI で count/CLEAR を描画 — headless では GAME_VIEW_NOT_FOCUSED でスクリーンショット不可、状態フラグで検証)
- R リセット(再初期化): count=0, clearShown=False, Player 位置 (0,0.5,-7)+速度 0, Item_1..5 すべて activeSelf=1 かつ位置 -4/-1/2/5/8 復元 ✓ (B5)

## 検出した不具合と修正(root cause)
1. PlayerController が毎フレーム速度をゼロ化 → 重力上書きでボールが浮遊/床通過。FixedUpdate 化+入 lực なし時は横方向のみ停止(垂直速度=重力維持)で修正 ✓
2. R キー: bridge の入力は device state 直書きで `wasPressedThisFrame` が発火しない → isPressed の立ち上がりエッジ検出(FixedUpdate)へ変更 ✓
3. ResetAll の `GameObject.Find` は inactive オブジェクトを見つけられない → Awake で item 参照を保持するよう修正 ✓

## 追加（ユーザー報告後）— 落下時の自動復帰
- 報告「落下しても戻らない」→ 現行実装の復元手段はRリセットのみ(仕様)、跳ね返りはない(restitution 0)。落下テスト: y=3から落下→床で静止 ✓(トリガー誤設定修正後)
- ユーザー選択により追加: 床縁/貫通で y<-1 になった初期位置へ自動復帰(count保持)。検証: Item_1取得(count=1)→(10.5,-3,0)テレポート→自動復帰 (0,0.5,-7), count=1保持, 速度0 ✓

## 30秒制限（ユーザー追加依頼）
- 設計: OnGUIに残り時間表示、TIME UPでcounting停止（movementは継続）、Rリセットでタイマーも再初期化(30s)。count/collected/CLEARはtimeoutで消さない
- Play検証: t=3s→26.8 / R→30再初期化(読み28.7=経過分) ✓ / timeout後item通過もcount 0のまま(guard) ✓ / Rで復帰しcounting再開(count=1) ✓
- OnGUI表示(TIME UP/Time)はheadlessではスクリーンショット不可、フラグ+反射読みで検証

## 未完了
- UI の視覚確認(OnGUI スクリーンショット)は GUI Editor セッションが必要(現状 headless)。ロジック(0→5、CLEAR 表示条件)はフラグレベルで検証済み。
