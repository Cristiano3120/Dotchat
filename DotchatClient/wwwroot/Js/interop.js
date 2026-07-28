window.createBlobUrl = async (streamRef) =>
{
    const arrayBuffer = await streamRef.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    return URL.createObjectURL(blob);
};

window.limitDateYear = (el) => {
    el.addEventListener('input', () => {
        const parts = el.value.split('-'); // Format: yyyy-MM-dd
        if (parts[0] && parts[0].length > 4) {
            parts[0] = parts[0].slice(0, 4);
            el.value = parts.join('-');
            el.dispatchEvent(new Event('change', { bubbles: true }));
        }
    });
};