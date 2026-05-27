// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(function () {
    function setLikeButtonState(button, isLiked, likeCount) {
        const icon = button.querySelector("i");
        const likeCountElement = button.querySelector(".post-like-count");

        if (icon) {
            icon.className = isLiked ? "bi bi-heart-fill fs-5" : "bi bi-heart fs-5";
        }

        if (likeCountElement) {
            likeCountElement.textContent = likeCount;
        }

        button.classList.remove("text-danger", "text-secondary");
        button.classList.add(isLiked ? "text-danger" : "text-secondary");
        button.setAttribute("data-liked", isLiked ? "true" : "false");
    }

    async function togglePostLike(button) {
        const postId = button.getAttribute("data-post-id");

        if (!postId || button.getAttribute("data-loading") === "true") {
            return;
        }

        button.setAttribute("data-loading", "true");
        button.disabled = true;

        try {
            const response = await fetch("/Post/ToggleLike", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
                },
                body: `postId=${encodeURIComponent(postId)}`
            });

            if (!response.ok) {
                return;
            }

            const data = await response.json();

            if (!data || data.success !== true) {
                return;
            }

            setLikeButtonState(button, data.isLiked, data.likeCount);
        } catch (error) {
            console.error("Toggle like error:", error);
        } finally {
            button.removeAttribute("data-loading");
            button.disabled = false;
        }
    }

    document.addEventListener("click", function (event) {
        const button = event.target.closest(".post-like-btn");

        if (!button) {
            return;
        }

        event.preventDefault();
        togglePostLike(button);
    });
})();
