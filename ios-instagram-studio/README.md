# InstagramStudio (iOS SwiftUI)

A starter iOS app that turns photos into Instagram-ready **posts** (1:1 and 4:5) or **reels** (9:16).

## Features
- Import image from photo library.
- Pick output format:
  - Square Post (1080x1080)
  - Portrait Post (1080x1350)
  - Reel Cover (1080x1920)
- Drag to reposition image inside the frame.
- Pinch to zoom image.
- Optional caption and hashtag fields.
- Render final export with a gradient background and text overlay.
- Save output image back to Photos.

## Project Structure
- `InstagramStudioApp.swift`: App entry point.
- `Models/InstagramFormat.swift`: Format definitions and dimensions.
- `Models/RenderSettings.swift`: User-selected options and metadata.
- `ViewModels/EditorViewModel.swift`: State + render/save logic.
- `Services/ImageRendererService.swift`: Core compositing logic.
- `Views/EditorView.swift`: Main editor UI.
- `Views/Components/FormatPicker.swift`: Segmented format selector.
- `Views/Components/CanvasPreview.swift`: Interactive preview canvas.

## Notes
This is a SwiftUI scaffold designed to be dropped into an Xcode iOS app target (iOS 17+ recommended).
