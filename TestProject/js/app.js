async function fetchDirectoryContents(path) {
    try {
        if (!path)
            path = '';

        const response = await fetch(`/api/filebrowser/getcontents?path=${encodeURIComponent(path)}`);
        if (!response.ok) {
            throw new Error('Error retrieving the requested data.');
        }
        return await response.json();
    } catch (error) {
        console.error('Fetch error:', error);
    }
}