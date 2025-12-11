// Smooth scrolling for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    });
});

// Auto-scroll to upload section if there's an error message
window.addEventListener('load', function () {
    console.log('Page fully loaded');

    const uploadSection = document.getElementById('upload');
    console.log('Upload section found:', uploadSection);

    if (uploadSection) {
        const errorAlert = uploadSection.querySelector('.alert-danger');
        console.log('Error alert found:', errorAlert);

        if (errorAlert) {
            console.log('Scrolling to upload section...');

            // Use a longer delay and multiple scroll attempts
            setTimeout(function () {
                uploadSection.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center'
                });
            }, 500);
        }
    }
});