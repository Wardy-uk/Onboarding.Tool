import SwiftUI
import PhotosUI

struct EditorView: View {
    @StateObject private var viewModel = EditorViewModel()

    var body: some View {
        NavigationStack {
            ScrollView {
                VStack(spacing: 16) {
                    CanvasPreview(sourceImage: viewModel.selectedImage, settings: $viewModel.settings)
                        .padding(.top, 8)

                    VStack(spacing: 12) {
                        FormatPicker(selection: $viewModel.settings.format)

                        PhotosPicker(selection: $viewModel.pickerItem, matching: .images, preferredItemEncoding: .automatic) {
                            Label("Import Image", systemImage: "photo.on.rectangle")
                                .frame(maxWidth: .infinity)
                        }
                        .buttonStyle(.borderedProminent)
                        .task(id: viewModel.pickerItem) {
                            await viewModel.handlePhotoSelection()
                        }

                        VStack(alignment: .leading, spacing: 8) {
                            Text("Caption")
                                .font(.subheadline.weight(.semibold))
                            TextField("Describe your post", text: $viewModel.settings.caption, axis: .vertical)
                                .textFieldStyle(.roundedBorder)
                        }

                        VStack(alignment: .leading, spacing: 8) {
                            Text("Hashtags")
                                .font(.subheadline.weight(.semibold))
                            TextField("#reels #instagood", text: $viewModel.settings.hashtags)
                                .textFieldStyle(.roundedBorder)
                        }

                        HStack {
                            Button {
                                viewModel.generateOutput()
                            } label: {
                                Label("Generate", systemImage: "wand.and.stars")
                                    .frame(maxWidth: .infinity)
                            }
                            .buttonStyle(.borderedProminent)
                            .disabled(viewModel.isRendering)

                            Button {
                                viewModel.saveToPhotos()
                            } label: {
                                Label("Save", systemImage: "square.and.arrow.down")
                                    .frame(maxWidth: .infinity)
                            }
                            .buttonStyle(.bordered)
                            .disabled(viewModel.renderedImage == nil)
                        }

                        if let rendered = viewModel.renderedImage {
                            VStack(alignment: .leading, spacing: 8) {
                                Text("Export Preview")
                                    .font(.subheadline.weight(.semibold))
                                Image(uiImage: rendered)
                                    .resizable()
                                    .scaledToFit()
                                    .clipShape(RoundedRectangle(cornerRadius: 14, style: .continuous))
                            }
                        }

                        Text(viewModel.statusMessage)
                            .font(.footnote)
                            .foregroundStyle(.secondary)
                            .frame(maxWidth: .infinity, alignment: .leading)
                    }
                }
                .padding()
            }
            .navigationTitle("Instagram Studio")
        }
    }
}

#Preview {
    EditorView()
}
