export function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      const result = reader.result as string;
      const base64 = result.split(",")[1];
      resolve(base64);
    };
    reader.onerror = (error) => reject(error);
  });
}

export function base64ToFile(base64: string, fileName: string): File {
  const byteString = atob(base64);
  const arrayBuffer = new ArrayBuffer(byteString.length);
  const intArray = new Uint8Array(arrayBuffer);

  for (let i = 0; i < byteString.length; i++) {
    intArray[i] = byteString.charCodeAt(i);
  }

  let mimeType = "image/png";
  if (intArray[0] === 0xff && intArray[1] === 0xd8 && intArray[2] === 0xff) {
    mimeType = "image/jpeg";
  } else if (
    intArray[0] === 0x89 &&
    intArray[1] === 0x50 &&
    intArray[2] === 0x4e &&
    intArray[3] === 0x47
  ) {
    mimeType = "image/png";
  }

  return new File([intArray], fileName, { type: mimeType });
}
