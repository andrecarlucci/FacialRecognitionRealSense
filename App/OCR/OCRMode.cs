namespace App.OCR {
    public enum OCRMode {
        /// <summary>
        /// Perform a full page OCR
        /// </summary>
        FullPage,

        /// <summary>
        /// Detect the text region before applying OCR.
        /// </summary>
        TextDetection
    }
}
