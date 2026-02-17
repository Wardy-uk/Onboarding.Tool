import SwiftUI

struct FormatPicker: View {
    @Binding var selection: InstagramFormat

    var body: some View {
        Picker("Format", selection: $selection) {
            ForEach(InstagramFormat.allCases) { format in
                Text(format.title).tag(format)
            }
        }
        .pickerStyle(.segmented)
    }
}
