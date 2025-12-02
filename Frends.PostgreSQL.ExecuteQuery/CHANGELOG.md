# Changelog

## [2.0.0] - 2025-12-02
### Added
- Added `ExecuteType` parameter to Input class with options: Auto (default), ExecuteReader, and NonQuery.
- Added support for INSERT/UPDATE/DELETE statements with RETURNING clause to return actual column values instead of just AffectedRows.
- Added `ExecuteTypes` enum to provide explicit control over query execution behavior.

### Changed
- Modified query execution logic to use ExecuteType parameter instead of simple string parsing.
- Auto mode now checks reader.FieldCount to determine if data is returned, providing more reliable detection than keyword matching.
- Transaction handling now only applies to write operations (Auto and NonQuery modes), not read-only queries (ExecuteReader mode).

### Fixed
- Fixed issue where INSERT/UPDATE/DELETE with RETURNING clause only returned AffectedRows instead of the actual returned column values.

## [1.1.0] - 2024-08-23
### Changed
- Updated the Newtonsoft.Json package to version 13.0.3 and the Npgsql package to version 8.0.3.

## [1.0.1] - 2023-02-02
### Fixed
- Fixed memory leak issue by adding cleanup method to the main class.

## [1.0.0] - 2022-10-13
### Added
- Initial implementation