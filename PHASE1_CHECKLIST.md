# Phase 1 - Manual Checklist

- [ ] App starts -> Identity dialog appears (blocking)
- [ ] Identity: Enter name and staff ID, click OK -> MainWindow opens
- [ ] Identity: Click Cancel -> App exits
- [ ] Identity: Leave fields empty, OK disabled or shows error
- [ ] Settings persist: device_id in %APPDATA%\WORKAUDIT\user_settings.json
- [ ] base_dir and base_dir\inbox folders created on first run
- [ ] base_dir defaults to Documents\WORKAUDIT_Docs when not set
- [ ] Ctrl+, opens Preferences dialog
- [ ] Preferences: General tab - base_dir, OCR language
- [ ] Preferences: Appearance tab - theme (dark/light/midnight/modern_dark), language (en/ar)
- [ ] Preferences: Browse for base_dir works
- [ ] Preferences: Save persists to user_settings.json
- [ ] Language "ar" applies FlowDirection RightToLeft to MainWindow
- [ ] SessionUser (name, staffId) available in memory for session
