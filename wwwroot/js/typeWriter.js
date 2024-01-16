async function typeContent(elementToWrite, textToWrite) {
    // Fetch all cells/elements and set their data-content attributes
    const startingText = elementToWrite.innerHTML || '';
    const placeholder = '<p style="visibility: hidden;">' + startingText + ' ' + textToWrite + '</p>';
    elementToWrite.setAttribute('data-content', textToWrite);
    elementToWrite.innerHTML = placeholder;

    async function typeContentIntoElement(element) {
        //console.log(startingText)
        //console.log(textToWrite)
        const content = element.getAttribute('data-content') || '';
        let text = '';
        let charIndex = 0;
        const typingSpeed = 25;
        var timeToWait = content.length * typingSpeed

        async function typeChar() {
            if (charIndex < content.length) {
                text += content.charAt(charIndex);
                element.innerHTML = `${startingText + text}`;
                charIndex++;
                setTimeout(typeChar, typingSpeed)
                await new Promise(r => setTimeout(r, timeToWait + 250))
            }
        }

        await typeChar();
    }

    await typeContentIntoElement(elementToWrite);
}