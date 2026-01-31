# Phase 2 - Manual Checklist

## Database
- [ ] workaudit.db and workaudit_audit.db created in base_dir on first run
- [ ] documents table has correct schema (id, uuid, file_path, section, status, etc.)
- [ ] audit_log table has hash chain (prev_hash)

## Workspace
- [ ] 4 columns: Filter + Tabs | Web | Preview | Inspector
- [ ] Files tab: file tree rooted at base_dir, expands on demand
- [ ] Documents tab: table shows ID, Type, Date, Status (limit 500)
- [ ] Load button applies filters (section, status, etc.)
- [ ] Preview: PDF/image from file_path; Ctrl+wheel zoom
- [ ] Inspector: status dropdown, notes, Save Notes, Mark Reviewed, Ready for Audit, Apply
- [ ] Selecting doc updates Properties Dock

## Search
- [ ] Search query box + filters (section, status)
- [ ] Search uses SQL LIKE on ocr_text, snippet, notes, file_path
- [ ] Table: id, type, date, source, capture, confidence, path
- [ ] Save notes, Delete, Open file
- [ ] search_panel_visible (Preferences) hides/shows right panel

## Audit
- [ ] update_notes, update_status, mark_reviewed, delete append to audit_log
- [ ] audit_forward: JSONL appended to %APPDATA%\WORKAUDIT\audit_forward\audit.jsonl
- [ ] Policy: forward before DB insert; if forward fails, no insert

## Seed Tool
- [ ] Tools → Developer → Seed sample documents
- [ ] File picker: select up to 5 files, insert as documents

## Shortcuts
- [ ] Ctrl+Shift+N saves notes for selected doc
- [ ] F5 refreshes current page (Workspace/Search)
