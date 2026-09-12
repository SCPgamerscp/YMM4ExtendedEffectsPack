# YMM4 Extended Effects Pack

ゆっくりMovieMaker4 向けの統合映像エフェクト／場面切り替えパックです。

[#1](https://github.com/ailingmeng585-ship-it/YMM4ExtendedEffectsPack/pull/1) のネイティブ場面切り替え・実フレーム残像・プリズムウィップと、[#2](https://github.com/ailingmeng585-ship-it/YMM4ExtendedEffectsPack/pull/2) の 34 種日本語エフェクトを **1 本の DLL** に統合しています。

| 収録 | 数 |
|---|---|
| 映像エフェクト | **35**（仕様 14 + リサーチ 20 + プリズムウィップ） |
| ネイティブ場面切り替え | **16**（場面切り替え一覧に `EEP /` で登録） |

進行度スライダを持つ映像エフェクトは、これまで通りキーフレーム 0→100% でもトランジションとして使えます。ネイティブ版はアイテム長に合わせて自動進行し、緩急／加減速を付けられます。

## 映像エフェクト（35）

### 仕様書（14）

| エフェクト | 場面切替 | 概要 |
|---|:---:|---|
| 劇場カーテン | あり | ベルベットのドレープ、金フリンジ、両開き／片引き |
| まばたき | あり | 湾曲まぶた、ピンボケ、自動パチパチ |
| ピクセル化 | — | 無段階サイズ、四角／丸／六角、FC／GB 減色 |
| 鼓動 | — | ドッ・クン二段脈動、BPM、色収差 |
| 七色ゲーミング | — | 色相回転、虹ウェーブ、エッジネオン |
| ブロックウェーブ | あり | 格子分割＋水平／垂直／放射波 |
| 炎（画像炎化） | — | 画像そのものが立ち昇る変形 |
| ゴッドレイ | — | 多条光線＋雲間スリット |
| マトリックス | あり | ネオングリーンのコードレイン |
| サンダー | — | 落雷フラッシュ／帯電オーラ |
| ズームブラー＆シェイク | — | ショート動画定番の衝撃揺れ |
| フィルムバーン | あり | 8mm 風の光漏れ切替 |
| モーションゴースト | — | **実過去フレーム**の RGB 残像（シークで履歴リセット） |
| レトロVHS | — | 走査線・トラッキング・同期ズレ |

### リサーチ追加（20）+ PR #1 固有（1）

[RESEARCH.md](RESEARCH.md) 参照。

| エフェクト | 場面切替 | 出典 |
|---|:---:|---|
| タイムワープスキャン | — | CapCut / TikTok |
| サイバーネオン輪郭 | — | AE / CapCut |
| インクスプラッシュ | あり | Premiere / CapCut |
| 魚眼レンズ | — | GoPro / CapCut |
| 集中線 | — | 漫画／ゆっくり実況 |
| アナモルフィックフレア | — | 映画レンズ |
| カレイドスコープ | — | MV |
| 紙破り | あり | アナログ切替 |
| 古フィルム | — | 映写機 |
| ソフトブルーム | — | シネマ |
| ホイップパン | あり | CapCut / Reels |
| グリッチシフト | あり | CapCut Glitch Pack |
| ズームパンチ | あり | TikTok intro |
| アイリスワイプ | あり | 古典／立ち絵 |
| ルマメルト | あり | MV / CapCut |
| 衝撃波 | あり | 実況・スキル |
| ハーフトーン | — | 漫画・印刷 |
| 水面リップル | — | 回想・夢 |
| スポットライト | あり | 舞台 |
| 渦巻き | あり | 魔法・サイケ |
| **プリズムウィップ** | あり | PR #1（ミラー端＋色収差パン。ホイップパンとは別物） |

## 導入

1. YMM4 を v4.47 以降（.NET 10）に更新し、テスト用プロジェクトを用意する
2. ビルドした `YMM4.ExtendedEffectsPack.dll` を `user/plugin/YMM4.ExtendedEffectsPack/` に置く（フォルダ名は `plugin`）
3. YMM4 を再起動し、「設定 → プラグイン」で読み込みを確認
4. 映像エフェクトで「拡張エフェクト」、場面切り替えで `EEP` を検索

## ビルド（Windows）

前提: YMM4 v4.47+、.NET 10 SDK、Windows SDK の `fxc.exe`。

1. `Directory.Build.props` の `YMM4DirPath` を `YukkuriMovieMaker4.exe` のあるフォルダに合わせる（末尾 `\` 必須）
2. `fxc.exe` を PATH に入れる
3. `dotnet build -c Release YMM4.ExtendedEffectsPack/YMM4.ExtendedEffectsPack.csproj`
4. 成功すると本体の `user/plugin/YMM4.ExtendedEffectsPack/` に DLL がコピーされる

パッケージ化:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\build.ps1 -Ymm4Dir "C:\Program Files\YukkuriMovieMaker4"
```

構造テスト（Python 標準ライブラリのみ、Windows 不要）:

```
python -m unittest discover -s tests -p "test_*.py" -v
```

GPU がシェーダーを読めない場合、当該エフェクトは無処理でフォールスルーします。

## 統合で足したもの

- **PR #2 を土台**: エフェクトごと固有 HLSL と Direct2D `CustomEffect` 型（シェーダー登録の衝突なし）、日本語 UI
- **PR #1 から移植**:
  - `ITransitionPlugin` によるネイティブ場面切り替え 16 種（緩急／加減速付き、進行度は自動）
  - `FrameHistory` — モーションゴーストが実際の過去フレームを保持（インスタンスあたり目安 128 MiB）
  - プリズムウィップ（ミラー端処理＋ズーム。ホイップパンと併用可）
  - `.ymme` 梱包スクリプト

## 制約

- 色のキーフレーム補間は未対応（色は固定 `ColorPicker`）
- モーションゴーストは連続再生が前提。シーク・サイズ変更・遅延変更で履歴を破棄する
- カーテンはシェーダーによる布風の陰影であり、物理布シミュレーションではない
- 点滅・虹色・雷・フラッシュは光過敏性発作のリスクあり。低速・低強度から調整すること
- AviUtl / EXO 変換は非対応
- Windows での SM5 実機描画・YMM4 編集画面／書き出しは、導入後にテスト用プロジェクトで確認してください

## プロジェクト構成

```
Directory.Build.props
YMM4.ExtendedEffectsPack/
  Common/          プロセッサ・FrameHistory・場面切替土台
  Effects/         [VideoEffect] と CustomEffect
  Shaders/         HLSL ps_5_0（fxc で .cso を埋め込み）
  Transitions/     [ITransitionPlugin] 16 種
tests/             構造テスト
tools/build.ps1    .ymme 梱包
```

## ライセンス

MIT
