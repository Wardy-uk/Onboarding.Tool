import SwiftUI

struct CanvasPreview: View {
    let sourceImage: UIImage?
    @Binding var settings: RenderSettings

    @GestureState private var gestureScale: CGFloat = 1
    @GestureState private var dragTranslation: CGSize = .zero

    var body: some View {
        GeometryReader { geometry in
            ZStack {
                LinearGradient(
                    colors: [Color.purple.opacity(0.8), Color.pink.opacity(0.75), Color.orange.opacity(0.72)],
                    startPoint: .topLeading,
                    endPoint: .bottomTrailing
                )

                if let sourceImage {
                    Image(uiImage: sourceImage)
                        .resizable()
                        .scaledToFit()
                        .scaleEffect(settings.imageScale * gestureScale)
                        .offset(x: settings.imageOffset.width + dragTranslation.width,
                                y: settings.imageOffset.height + dragTranslation.height)
                        .gesture(combinedGesture)
                } else {
                    ContentUnavailableView("No Image", systemImage: "photo", description: Text("Import from your photo library"))
                        .foregroundStyle(.white)
                }

                VStack {
                    Spacer()
                    Rectangle()
                        .fill(.black.opacity(0.35))
                        .frame(height: geometry.size.height * 0.24)
                        .overlay(alignment: .leading) {
                            VStack(alignment: .leading, spacing: 4) {
                                Text(settings.caption.isEmpty ? "Your caption" : settings.caption)
                                    .font(.headline)
                                    .lineLimit(1)
                                Text(settings.hashtags.isEmpty ? "#reels #post" : settings.hashtags)
                                    .font(.subheadline)
                                    .lineLimit(1)
                            }
                            .foregroundStyle(.white)
                            .padding(.horizontal, 14)
                        }
                }
            }
            .clipShape(RoundedRectangle(cornerRadius: 20, style: .continuous))
            .overlay(
                RoundedRectangle(cornerRadius: 20, style: .continuous)
                    .stroke(.white.opacity(0.28), lineWidth: 2)
            )
        }
        .aspectRatio(settings.format.aspectRatio, contentMode: .fit)
        .animation(.snappy, value: settings.format)
    }

    private var combinedGesture: some Gesture {
        SimultaneousGesture(
            MagnificationGesture()
                .updating($gestureScale) { value, state, _ in
                    state = value
                }
                .onEnded { value in
                    settings.imageScale = max(0.6, min(5.0, settings.imageScale * value))
                },
            DragGesture()
                .updating($dragTranslation) { value, state, _ in
                    state = value.translation
                }
                .onEnded { value in
                    settings.imageOffset = CGSize(
                        width: settings.imageOffset.width + value.translation.width,
                        height: settings.imageOffset.height + value.translation.height
                    )
                }
        )
    }
}
