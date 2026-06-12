# Design principles

Kitsune is shaped by a few practical priorities that inform API and architecture decisions.

## Feel first

Gameplay feel and iteration speed outweigh feature checklists. Core stays lean so bridges and games can tune responsiveness without fighting the engine.

## Balance and flow

Systems should compose predictably: clear lifecycle hooks, explicit ownership (scenes own entities), and query paths that stay obvious as projects grow.

## Ma (間)

Leave room between systems. Optional bridges, granular packages, and deferred features (scene stacks, layers, particles) keep the core surface small until there is a concrete need.

## Modern C#, Monocle-informed

Concepts follow proven 2D engine patterns — Scene/Entity/Component, tags, colliders — reimplemented with nullable reference types, file-scoped namespaces, and Foster integration rather than API parity with Monocle.

## Layered growth

Core is genre-agnostic. Gameplay bridges (platformer, etc.) and tooling (editor) ship as separate projects so consumers install only what they need.