# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [1.0.4] — 2025-09-23
### Fixed
- WebGL template naming

## [1.0.4] — 2025-09-22
### Added
- Ability to create mission descriptions and export them to the admin panel (first iteration, improvements planned for future versions).
- Commands for working with the Strategic Betting mode:
  - `GetGameSessionInfoRequest` — retrieves information about the current game session type (Strategic Betting or Pay to Play).
  - `GetBettingSessionMissionsRequest` — retrieves information about SB missions, including description, bet amount, difficulty, and bet coefficient.
  - `SendStrategicBettingResultRequest` — sends the result of an SB game session, including mission ID and result.
- Commands for handling in-game Transactions — a mechanism that processes requests to continue playing after Game Over or to restart, handled directly by the system instead of the game client.

### Fixed
- Minor bug fixes.

## [1.0.3] — 2025-07-17
### Added
- Plugin version display in the WebGL Template.
- Automatic script for version injection during WebGL build.
- Example EditorWindow to display plugin information.
- Added logic for command dispatch when reconnecting to the server.

### Changed
- Documentation updated.

### Fixed
- Minor build-related bug fixes.

## [1.0.2] — 2025-06-23
### Fixed
- Fixed credits variable issue — switched from integer to float.
- Fixed reconnection issue.

## [1.0.1] — 2025-05-29
### Added
- Added Developer Token Setup.
- Added Newtonsoft Dependency Installer.

## [1.0.0] — 2025-05-15
### Added
- First stable release of the plugin.
