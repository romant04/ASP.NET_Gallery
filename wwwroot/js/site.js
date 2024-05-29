/* -- user modal -- */
const user = document.getElementsByClassName("user")
const userModal = document.getElementById("user-modal")

userModal.style.display = "none";

for (let i = 0; i < user.length; i++) {
    user[i].addEventListener("click", e => {
        console.log(user[1].classList)
        if (userModal.style.display == "none") {
            userModal.style.display = "flex";
            user[1].classList.remove("fa-caret-up")
            user[1].classList.add("fa-caret-down")
        }
        else if (userModal.style.display == "flex") {
            userModal.style.display = "none";
            user[1].classList.add("fa-caret-up")
            user[1].classList.remove("fa-caret-down")
        }
    })
}

/* -- back-button -- */
const back = document.getElementById("back")

back.addEventListener("click", () => {
    history.back();
})