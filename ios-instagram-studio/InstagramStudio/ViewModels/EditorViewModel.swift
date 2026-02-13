import SwiftUI
import PhotosUI

@MainActor
final class EditorViewModel: ObservableObject {
    @Published var settings = RenderSettings()
    @Published var selectedImage: UIImage?
    @Published var renderedImage: UIImage?
    @Published var pickerItem: PhotosPickerItem?
    @Published var isRendering = false
    @Published var statusMessage = "Import an image to start"

    private let renderer = ImageRendererService()

    func handlePhotoSelection() async {
        guard let pickerItem else { return }

        do {
            if let data = try await pickerItem.loadTransferable(type: Data.self),
               let image = UIImage(data: data) {
                selectedImage = image
                renderedImage = nil
                statusMessage = "Image ready. Adjust and export."
            }
        } catch {
            statusMessage = "Could not import image."
        }
    }

    func generateOutput() {
        guard let selectedImage else {
            statusMessage = "Please import an image first."
            return
        }

        isRendering = true
        renderedImage = renderer.renderImage(from: selectedImage, settings: settings)
        isRendering = false
        statusMessage = renderedImage == nil ? "Render failed." : "Rendered successfully."
    }

    func saveToPhotos() {
        guard let renderedImage else {
            statusMessage = "Nothing to save."
            return
        }

        UIImageWriteToSavedPhotosAlbum(renderedImage, nil, nil, nil)
        statusMessage = "Saved to Photos."
    }
}
