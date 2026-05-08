---
name: layer-core
description: Core層を実装するためのskill
---

# Core Layer

## 目的

Coreを基本的なドメインの中心として扱い、閉じ込める。

## Coreの本質

Coreは従来のDomain層に相当する存在である。ゲームのルールと状態遷移を集約する役割を持つ。`MonoBehaviour` に依存せず、外部I/OやUI都合は持ち込まない。外部システムとの境界が必要な場合はCore内にインターフェースを定義し、実装はInfrastructureへ委譲する。Coreは「何が正しいか」を決める層であり、「どう表示するか」「どう保存するか」は決めない。ただし、Unity都合でCoreに閉じ込めづらい処理、Core層に閉じ込めるメリットが薄い処理はActorへ置くことを許容する。

## ルール

- `MonoBehaviour` は継承しない。
- 他の層へは依存しない。
- 外部I/Oの境界はインターフェースをCore内に定義し、実装詳細はInfrastructureに委譲する。
- Unity APIや外部ライブラリは必要な範囲でのみ使う。
- ドメイン判断はメソッド内に閉じ込め、呼び出し側へドメインを漏らさない。
