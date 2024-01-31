async function isApplicationSetUp() {
    try {
        let response = await fetch('api/filebrowser/ishomedirectorysetup');
        let fetchData = await response.json();

        return fetchData.isApplicationSetUp;
    }
    catch (error) {
        console.error('Fetch error:', error);
        return false;
    }
}

async function fetchDirectoryContents(path) {
    try {
        if (!path)
            path = '';

        const response = await fetch(`/TestProject/api/filebrowser/getcontents?path=${encodeURIComponent(path)}`);
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
    console.log(pathArray);

    //concat strings to provide single update to DOM
    let fullPath = '';
    let breadcrumbHTML = `<ul class="breadcrumbNavList"><li><a onclick="loadDirectory('/')" href="#/">Home</a></li>`;
    for (let dir of pathArray) {
        fullPath += dir + '/';
        let link = ` / <li>
                            <a onclick="loadDirectory('${fullPath}')" 
                                href="#/${fullPath}">
                                    ${dir}
                            </a>
                       </li>`;
        breadcrumbHTML += link;
    }
    breadcrumbsContainer.innerHTML = breadcrumbHTML + '</ul>';
}

function renderDirectoryContents(directory) {
    const directoryContentsContainer = document.getElementById('directory-contents');
    let directoryContentsHtml = '';

    // Add subdirectories
    if (Array.isArray(directory.subdirectories)) {
        directoryContentsHtml += directory.subdirectories
            .map(subDir => `
                <div class="subdirectory-item" onclick="loadDirectory('${subDir.path}')">
                    <span class="folder-icon">📁</span> <strong>${subDir.name}</strong>
                </div>`)
            .join('');
    }

    // Add files
    if (Array.isArray(directory.files)) {
        directoryContentsHtml += directory.files
            .map(file =>
                `<div class="file-item" onclick="downloadFile('${file.path}')"><span class="file-icon">📄</span> ${file.name}</div>`)
            .join('');
    }

    directoryContentsContainer.innerHTML = directoryContentsHtml;

    // Update footer
    updateFooter(directory.subdirectories.length, directory.files.length);
}

function updateFooter(dirCount, fileCount) {
    const footer = document.getElementById('footer');
    footer.textContent = `Directories: ${dirCount}, Files: ${fileCount}`;
}

function downloadFile(filePath) {
    window.open('/TestProject/api/file/download?filePath=' + encodeURIComponent(filePath), '_blank');
}

async function loadDirectory(path) {
    fetchDirectoryContents(path).then(directoryData => {
        renderDirectoryContents(directoryData); // Render contents
        updateBreadcrumbs(path.split('/').filter(Boolean)); // Update breadcrumbs
    });
}