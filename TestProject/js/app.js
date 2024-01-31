async function fetchDirectoryContents(path) {
    try {
        if (!path)
            path = '';

        const response = await fetch(`api/filebrowser/getcontents?path=${encodeURIComponent(path)}`);
        if (!response.ok) {
            throw new Error('Error retrieving the requested data.');
        }
        return await response.json();
    } catch (error) {
        console.error('Fetch error:', error);
        return { errorMessage: "There was an error retrieving the directory data." }; 
    }
}

function updateBreadcrumbs(pathArray) {
    const breadcrumbsContainer = document.getElementById('breadcrumbs');
    breadcrumbsContainer.innerHTML = '';

    let fullPath = '';
    for (let dir of pathArray) {
        fullPath += dir + '/';
        const link = document.createElement('a');
        link.href = '#';
        link.textContent = dir + ' / ';
        link.onclick = () => loadDirectory(fullPath);
        breadcrumbsContainer.appendChild(link);
    }
}

function renderDirectoryContents(directory) {
    const directoryContentsContainer = document.getElementById('directory-contents');
    directoryContentsContainer.innerHTML = '';

    // Add subdirectories
    if (Array.isArray(directory.subdirectories))
        for (let subDir of directory.subdirectories) {
            let dirElement = document.createElement('div');
            dirElement.innerHTML = `<span class="folder-icon">📁</span> <strong>${subDir.name}</strong>`;
            dirElement.onclick = () => loadDirectory(subDir.path);
            directoryContentsContainer.appendChild(dirElement);
        }

    // Add files
    if (Array.isArray(directory.files))
        for (let file of directory.files) {
            let fileElement = document.createElement('div');
            fileElement.innerHTML = `<span class="file-icon">📄</span> ${file.name}`;
            directoryContentsContainer.appendChild(fileElement);
        }

    // Update footer
    updateFooter(directory.subdirectories.length, directory.files.length);
}

function updateFooter(dirCount, fileCount) {
    const footer = document.getElementById('footer');
    footer.textContent = `Directories: ${dirCount}, Files: ${fileCount}`;
}

async function loadDirectory(path) {
    fetchDirectoryContents(path).then(directoryData => {
        renderDirectoryContents(directoryData); // Render contents
        updateBreadcrumbs(path.split('/').filter(Boolean)); // Update breadcrumbs
    });
}