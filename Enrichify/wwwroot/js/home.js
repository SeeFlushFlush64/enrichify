// File upload handling
const fileInput = document.getElementById('file');
const dropZone = document.getElementById('dropZone');
const fileName = document.getElementById('fileName');

if (fileInput && dropZone && fileName) {
    fileInput.addEventListener('change', function (e) {
        if (e.target.files.length > 0) {
            fileName.textContent = `Selected: ${e.target.files[0].name}`;
        }
    });

    // Drag and drop
    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.add('drag-over');
    });

    dropZone.addEventListener('dragleave', () => {
        dropZone.classList.remove('drag-over');
    });

    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.classList.remove('drag-over');

        const files = e.dataTransfer.files;
        if (files.length > 0 && files[0].name.endsWith('.csv')) {
            fileInput.files = files;
            fileName.textContent = `Selected: ${files[0].name}`;
        }
    });

    dropZone.addEventListener('click', () => {
        fileInput.click();
    });
}

// Smooth scrolling
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    });
});