const inputs = document.getElementsByClassName("uploadInput")

const titleLabel = document.querySelector("label#Title")
const descriptionLabel = document.querySelector("label#Description")
const orderLabel = document.querySelector("label#Order")

for (let i = 0; i < inputs.length; i++) {
    console.log(inputs[i])

    if (inputs[i].value != "") {
        if (i == 0) {
            titleLabel.style.fontSize = "0.85rem"
            titleLabel.style.bottom = "25px"
        }
        else if (i == 1) {
            descriptionLabel.style.fontSize = "0.85rem"
            descriptionLabel.style.bottom = "25px"
        }
        else if (i == 2) {
            orderLabel.style.fontSize = "0.85rem"
            orderLabel.style.bottom = "25px"
        }
    }

    inputs[i].addEventListener("focusout", e => {
        if (e.target.value != "") {
            if (i == 0) {
                titleLabel.style.fontSize = "0.85rem"
                titleLabel.style.bottom = "25px"
            }
            else if (i == 1) {
                descriptionLabel.style.fontSize = "0.85rem"
                descriptionLabel.style.bottom = "25px"
            }
            else if (i == 2) {
                orderLabel.style.fontSize = "0.85rem"
                orderLabel.style.bottom = "25px"
            }
        }
        else {
            if (i == 0) {
                titleLabel.style.fontSize = "1rem"
                titleLabel.style.bottom = "0px"
            }
            else if (i == 1) {
                descriptionLabel.style.fontSize = "1rem"
                descriptionLabel.style.bottom = "0px"
            }
            else if (i == 2) {
                orderLabel.style.fontSize = "1rem"
                orderLabel.style.bottom = "0px"
            }
        }
    })

    inputs[i].addEventListener("focus", () => {
        if (i == 0) {
            titleLabel.style.fontSize = "0.85rem"
            titleLabel.style.bottom = "25px"
        }
        else if (i == 1) {
            descriptionLabel.style.fontSize = "0.85rem"
            descriptionLabel.style.bottom = "25px"
        }
        else if (i == 2) {
            orderLabel.style.fontSize = "0.85rem"
            orderLabel.style.bottom = "25px"
        }
    })
}