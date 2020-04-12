extern "C"
{
	// initializes the Background Download plugin with any relevant data it needs.
	void init(const char *storagePath);
	
	// Starts a background download of the file at the given URL.
	long startDownload(char *url,char *savePath);

	// Cancels the ongoing background download with the given id.
	void cancelDownload(long id);
    
    // Returns the id of an ongoing background download operation with the given URL.
    // will return -1 in case no download operation was found.
    long getDownloadTask(const char* url);
	
	// Returns the download progess [0..1] of the ongoing background download with the given id.
	float getDownloadProgress(long id);

	// Returns the download status of the ongoing background download with the given id.
	int getDownloadStatus(long id);

	// Returns any errors in the ongoing background download with the given id.
	const char *getError(long id);
}
