function maxRows(imageCount) {
    if (window.innerWidth < 768) {
        console.log("row 1")
        return imageCount + 1
    }
    else if (window.innerWidth < 1200) {
        console.log("row 2")
        return Math.floor(imageCount / 2) + 1
    }
    else if (window.innerWidth < 1400) {
        console.log("row 3")
        return Math.floor(imageCount / 3) + 1
    }
    else if (window.innerWidth >= 1400) {
        console.log("row 4")
        return Math.floor(imageCount / 4) + 1
    }
}

function maxColumn() {
    if (window.innerWidth < 768) {
        console.log("col 0")
        return 1
    }
    else if (window.innerWidth < 1200) {
        console.log("col 2")
        return 2
    }
    else if (window.innerWidth < 1400) {
        console.log("col 3")
        return 3
    }
    else if (window.innerWidth >= 1400) {
        console.log("col 4")
        return 4
    }
}

const oldRows = []
const oldCols = []

function rerender() {
    const galeryItems = document.getElementsByClassName("galleryItem")

    const imageCounter = document.getElementById("imageCount")
    const imageCount = parseInt(imageCounter.value)

    for (let i = 0; i < galeryItems.length; i++) {
        const row = oldRows[i]
        const col = oldCols[i]

        if (row != "x") {
            console.log(row < maxRows(imageCount) ? row : maxRows(imageCount))
            galeryItems[i].style.gridRow = row < maxRows(imageCount) ? row : maxRows(imageCount)
        }
        if (col != "x") {
            console.log(col < maxColumn() ? col : maxColumn())
            galeryItems[i].style.gridColumn = col < maxColumn() ? col : maxColumn()
        }
    }
}

const galeryItemsx = document.getElementsByClassName("galleryItem")

for (let i of galeryItemsx) {
    oldRows.push(parseInt(i.style.gridRow.split("/")[0]) || "x")
    oldCols.push(parseInt(i.style.gridColumn.split("/")[0]) || "x")
}
rerender()

window.addEventListener("resize", () => {
    rerender()
})

