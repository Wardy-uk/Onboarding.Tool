# InstagramStudio (iOS SwiftUI)

A fully runnable iOS app project that turns photos into Instagram-ready **posts** (1:1 and 4:5) or **reels** (9:16).

## What is included
- `InstagramStudio.xcodeproj` (open directly in Xcode)
- SwiftUI app target with these features:
  - Import image from photo library
  - Pick output format:
    - Square Post (`1080x1080`)
    - Portrait Post (`1080x1350`)
    - Reel Cover (`1080x1920`)
  - Drag to reposition image inside the frame
  - Pinch to zoom image
  - Optional caption and hashtag fields
  - Render final export with a gradient background and text overlay
  - Save output image back to Photos

## Run on iPhone
1. Open `ios-instagram-studio/InstagramStudio.xcodeproj` in Xcode (16+ recommended).
2. Select the `InstagramStudio` target.
3. In **Signing & Capabilities**:
   - Enable **Automatically manage signing**
   - Pick your Apple Developer Team
   - Set a unique bundle identifier (default is `com.example.InstagramStudio`)
4. Connect your iPhone and choose it as the run destination.
5. Build and run.
6. Allow Photos access when prompted.

## Files
- `InstagramStudio/InstagramStudioApp.swift`: App entry point.
- `InstagramStudio/Models/InstagramFormat.swift`: Format definitions and dimensions.
- `InstagramStudio/Models/RenderSettings.swift`: User-selected options and metadata.
- `InstagramStudio/ViewModels/EditorViewModel.swift`: State + render/save logic.
- `InstagramStudio/Services/ImageRendererService.swift`: Core compositing logic.
- `InstagramStudio/Views/EditorView.swift`: Main editor UI.
- `InstagramStudio/Views/Components/FormatPicker.swift`: Segmented format selector.
- `InstagramStudio/Views/Components/CanvasPreview.swift`: Interactive preview canvas.
- `InstagramStudio/Info.plist`: App metadata + Photos permissions.
