const imgBoxes = document.getElementsByClassName('imgBox')

console.log(imgBoxes[1])

function resetState() {
    for (let i of imgBoxes) {
        i.style.filter = "brightness(100%)";
        i.childNodes.forEach(f => {
            if (f.tagName == "I") {
                f.style.display = "none"
            }
        })
    }
}

for (let i of imgBoxes) {
    i.addEventListener('click', e => {
        resetState()

        i.style.filter = "brightness(50%)"
        i.childNodes.forEach(f => {
            if (f.tagName == "I")
            {
                f.style.display = "block"
            }
        })
    })
}