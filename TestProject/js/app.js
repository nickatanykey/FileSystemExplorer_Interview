function loadInitialState() {
    let path = getDirectoryPath();

    if (!!path) {
        loadDirectory(path);
    } else {
        // Load default or root directory
        loadDirectory('/');
    }
}

async function isApplicationSetUp() {
    try {
        let response = await fetch('/api/browser/isappsetup');
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

        const response = await fetch(`/api/browser/getcontents?path=${encodeURIComponent(path)}`);
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

    updateDirectoryView(directory);

    updateFooter(directory.subdirectories.length, directory.files.length);
}

function updateDirectoryView(directory) {
    const directoryContentsContainer = document.getElementById('directoryContents');
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
               `<div class="file-item-wrapper">
                    <span class="file-icon" onclick="downloadFile('${file.path}')">📄 ${file.name}</span>
                    <span>${file.length / 1000} kb</span>
                    <span class="delete-file-link" onclick="deleteFile('${file.path}', '${file.name}')">delete</span>
                </div>`)
            .join('');
    }

    directoryContentsContainer.innerHTML = directoryContentsHtml;
}

function updateFooter(dirCount, fileCount) {
    document.getElementById('itemCountPanel').innerHTML = `<div>Directories: ${dirCount}, Files: ${fileCount}</div>`;
}

function downloadFile(filePath) {
    window.open('/api/file/download?filePath=' + encodeURIComponent(filePath), '_blank');
}

function updateUrl(path) {
    history.pushState({ path: path }, '', '#path=' + encodeURIComponent(path));
}

function getDirectoryPath() {
    let currentUrl = new URL(window.location.href);
    let dirPath = currentUrl.hash.match(/#path=(.*)/)?.[1];
    if (!dirPath)
        return null;

    return decodeURIComponent(dirPath);
}

let currentDirectory = {};

async function loadDirectory(path) {
    fetchDirectoryContents(path).then(directoryData => {
        currentDirectory = directoryData;
        renderDirectoryContents(currentDirectory);
        updateBreadcrumbs(path.split('/').filter(Boolean));
        updateUrl(path);
    });
}

async function deleteFile(fullFilePath) {
    fetch(`/api/file?fullFilePath=${encodeURIComponent(fullFilePath) }`, {
        method: 'DELETE'
    }).then(() => {
        currentDirectory.files = currentDirectory.files.filter(file => file.path != fullFilePath);
        renderDirectoryContents(currentDirectory);
    });

}

function deleteDirectory(path) {

}

function uploadFile() {
    let fileInput = document.getElementById('fileUpload');
    let file = fileInput.files[0];
    let formData = new FormData();
    formData.append('file', file);

    let path = getDirectoryPath();

    fetch('/api/file/upload?relativePath=' + encodeURIComponent(path), {
        method: 'POST',
        body: formData
    })
    .then(() => {
        currentDirectory.files.push({ path: path, name: file.name });
        renderDirectoryContents(currentDirectory);
    })
    .catch(error => console.log("Error", error));
}

async function search() {
    let searchText = document.getElementById("searchInput").value;
    searchText = encodeURIComponent(searchText);
    let response = await fetch(`/api/browser/search?searchText=${searchText}`)
    let searchResults = await response.json();
    renderSearchResults(searchResults);
}

function renderSearchResults(searchResults) {
    let searchResultsPanel = document.getElementById("searchResultsPanel");
    let searchResultHtml = '';
    console.log(searchResults);
    searchResultHtml += '<div><span class="folder-icon">📁</span> <span>Directories</span></div> <hr />';
    if (Array.isArray(searchResults.directories)) {
        searchResultHtml += searchResults.directories.map(dir => `<div class="search-result-item" onclick="loadDirectory('${dir.name}')">${dir.name}</div>`).join('');
    } else {
        searchResultHtml += '<div>None</div>';
    }

    searchResultHtml += '<div style="margin-top:20px;">📄 <span>Files</span></div> <hr />';
    if (Array.isArray(searchResults.files)) {
        searchResultHtml += searchResults.files
            .map(file =>
                `<div class="search-result-item" onclick="loadDirectory('${file.relativePath}')"><span class="file-icon"> ${file.name}</span></div>`
            )
            .join('');
    } else {
        searchResultHtml += '<div>None</div>';
    }

    searchResultsPanel.innerHTML = searchResultHtml;
    searchResultsPanel.style.display = 'block';
}

function clearSearch() {
    document.getElementById("searchInput").value = '';
    searchResultsPanel.innerHTML = '';
    searchResultsPanel.style.display = 'none';
}