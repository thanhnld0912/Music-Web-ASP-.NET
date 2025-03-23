let currentPostId = null;
const currentUserId = '@(userId != null ? userId : "null")';  // Chèn giá trị Razor dưới dạng chuỗi
let commentsStore = {};


function toggleProfileMenu(event) {
    event.stopPropagation();
    document.getElementById('profileMenu').classList.toggle('show');
}

function toggleSidebar(event) {
    event.stopPropagation();
    document.getElementById('sidebar').classList.toggle('collapsed');
}

function toggleChatBox(event, boxId) {
    event.stopPropagation();
    closeAll(event);
    document.getElementById(boxId).style.display = 'block';
}

function closeAll(event) {
    if (event && (event.target.closest('.chat-box') ||
        event.target.closest('.sidebar-button') ||
        event.target.closest('.post-options') ||
        event.target.closest('.avatar-container') ||
        event.target.closest('#comment-overlay'))) return;

    document.querySelectorAll('.chat-box, .post-menu, .profile-menu').forEach(el => el.style.display = 'none');

    // Don't close comment overlay if clicked inside it
    if (!event || !event.target.closest('#comment-overlay')) {
        closeCommentSection();
    }
}


function likePost(button, postId) {
    if (!currentUserId || currentUserId === "null") {
        alert("Vui lòng đăng nhập để thích bài viết");
        return;
    }

    const isActive = button.classList.contains('active');

    // Nếu đã dislike, xóa dislike
    const dislikeBtn = button.closest('.action-buttons-container').querySelector('.dislike-btn');
    if (dislikeBtn.classList.contains('active')) {
        dislikeBtn.classList.remove('active');
        const dislikeCountEl = dislikeBtn.querySelector('.count-badge');
        const currentDislikeCount = parseInt(dislikeCountEl.textContent);
        if (currentDislikeCount > 0) {
            dislikeCountEl.textContent = currentDislikeCount - 1;
        }
    }

    // Lấy số lượng like hiện tại
    const likeCountEl = button.querySelector('.count-badge');
    const currentCount = parseInt(likeCountEl.textContent);

    // Tạo AJAX request
    const xhr = new XMLHttpRequest();
    xhr.open("POST", `/Post/LikePost/${postId}`, true);
    xhr.setRequestHeader("Content-Type", "application/json");

    // Debug output
    console.log("Sending like request:", postId, currentUserId);

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                console.log("Like response:", xhr.responseText);
                // Toggle trạng thái active và cập nhật số lượng
                if (isActive) {
                    button.classList.remove('active');
                    if (currentCount > 0) {
                        likeCountEl.textContent = currentCount - 1;
                    }
                } else {
                    button.classList.add('active');
                    likeCountEl.textContent = currentCount + 1;
                }
            } else {
                console.error("Lỗi khi thích bài viết:", xhr.responseText);
                alert("Không thể thích bài viết. Vui lòng thử lại sau.");
            }
        }
    };

    xhr.send(JSON.stringify(parseInt(currentUserId)));
}

function dislikePost(button, postId) {
    if (!currentUserId || currentUserId === "null") {
        alert("Vui lòng đăng nhập để không thích bài viết");
        return;
    }

    const isActive = button.classList.contains('active');

    // Nếu đã like, xóa like
    const likeBtn = button.closest('.action-buttons-container').querySelector('.like-btn');
    if (likeBtn.classList.contains('active')) {
        likeBtn.classList.remove('active');
        const likeCountEl = likeBtn.querySelector('.count-badge');
        const currentLikeCount = parseInt(likeCountEl.textContent);
        if (currentLikeCount > 0) {
            likeCountEl.textContent = currentLikeCount - 1;
        }
    }

    // Lấy số lượng dislike hiện tại
    const dislikeCountEl = button.querySelector('.count-badge');
    const currentCount = parseInt(dislikeCountEl.textContent);

    // Tạo AJAX request
    const xhr = new XMLHttpRequest();
    xhr.open("POST", `/Post/DislikePost/${postId}`, true);
    xhr.setRequestHeader("Content-Type", "application/json");

    // Debug output
    console.log("Sending dislike request:", postId, currentUserId);

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                console.log("Dislike response:", xhr.responseText);
                // Toggle trạng thái active và cập nhật số lượng
                if (isActive) {
                    button.classList.remove('active');
                    if (currentCount > 0) {
                        dislikeCountEl.textContent = currentCount - 1;
                    }
                } else {
                    button.classList.add('active');
                    dislikeCountEl.textContent = currentCount + 1;
                }
            } else {
                console.error("Lỗi khi không thích bài viết:", xhr.responseText);
                alert("Không thể không thích bài viết. Vui lòng thử lại sau.");
            }
        }
    };

    xhr.send(JSON.stringify(parseInt(currentUserId)));
}

function loadAllCommentCounts() {
    const posts = document.querySelectorAll('.post');
    posts.forEach(post => {
        const postId = post.getAttribute('data-post-id');
        if (postId) {
            fetchCommentData(postId);
        }
    });
}

function fetchCommentData(postId) {
    const xhr = new XMLHttpRequest();
    xhr.open("GET", `/Post/GetComments/${postId}`, true);

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                try {
                    const comments = JSON.parse(xhr.responseText);

                    // Lưu comments vào cache
                    commentsStore[postId] = comments;

                    // Update comment count
                    updateCommentCountBadge(postId, comments.length);
                } catch (error) {
                    console.error("Lỗi khi phân tích comments:", error);
                }
            } else {
                console.error("Lỗi khi tải comments:", xhr.responseText);
            }
        }
    };

    xhr.send();
}

function toggleCommentSection(button, postId) {
    event.stopPropagation();

    if (!currentUserId) {
        alert("Please login to view and add comments");
        return;
    }

    currentPostId = postId;
    const commentOverlay = document.getElementById('comment-overlay');
    commentOverlay.style.display = 'flex';

    const commentsList = commentOverlay.querySelector('.fb-comments-list');
    commentsList.innerHTML = '';

    // Use cached comments if available
    if (commentsStore[postId] && commentsStore[postId].length > 0) {
        commentsStore[postId].forEach(comment => {
            const commentItem = createCommentElement(comment);
            commentsList.appendChild(commentItem);
        });
    } else {
        // If not cached yet, load them
        loadComments(postId, commentsList);
    }

    const commentInput = commentOverlay.querySelector('.fb-comment-input');
    if (commentInput) {
        commentInput.focus();
    }

    button.classList.add('active');
}

function loadComments(postId, commentsList) {
    const xhr = new XMLHttpRequest();
    xhr.open("GET", `/Post/GetComments/${postId}`, true);

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                try {
                    const comments = JSON.parse(xhr.responseText);

                    // Lưu comments vào cache
                    commentsStore[postId] = comments;

                    // Hiển thị comments
                    comments.forEach(comment => {
                        const commentItem = createCommentElement(comment);
                        commentsList.appendChild(commentItem);
                    });

                    // Update comment count badge
                    updateCommentCountBadge(postId, comments.length);
                } catch (error) {
                    console.error("Lỗi khi phân tích comments:", error);
                }
            } else {
                console.error("Lỗi khi tải comments:", xhr.responseText);
            }
        }
    };

    xhr.send();
}

function updateCommentCountBadge(postId, count) {
    const postElement = document.querySelector(`.post[data-post-id="${postId}"]`);
    if (postElement) {
        const commentCountEl = postElement.querySelector('.comment-btn .count-badge');
        if (commentCountEl) {
            commentCountEl.textContent = count;
        }
    }
}

function closeCommentSection() {
    document.getElementById('comment-overlay').style.display = 'none';

    if (currentPostId) {
        const postElement = document.querySelector(`.post[data-post-id="${currentPostId}"]`);
        if (postElement) {
            const commentBtn = postElement.querySelector('.comment-btn');
            if (commentBtn) {
                commentBtn.classList.remove('active');
            }
        }
    }

    currentPostId = null;
}

function submitComment(event) {
    if (!currentUserId) {
        alert("Vui lòng đăng nhập để bình luận");
        return;
    }

    const commentSection = event.target.closest('.fb-comment-section');
    const commentInput = commentSection.querySelector('.fb-comment-input');
    const commentText = commentInput.value.trim();

    if (commentText && currentPostId) {
        // Tạo comment data object
        const commentData = {
            content: commentText,
            userId: currentUserId
        };

        // Tạo AJAX request
        const xhr = new XMLHttpRequest();
        xhr.open("POST", `/Post/AddComment/${currentPostId}`, true);
        xhr.setRequestHeader("Content-Type", "application/json");

        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        const commentsList = commentSection.querySelector('.fb-comments-list');

                        // Lưu comment vào cache
                        if (!commentsStore[currentPostId]) {
                            commentsStore[currentPostId] = [];
                        }
                        commentsStore[currentPostId].push(response);

                        // Tạo và hiển thị comment mới
                        const commentItem = createCommentElement(response);
                        commentsList.appendChild(commentItem);

                        // Update comment count badge
                        updateCommentCountBadge(currentPostId, commentsStore[currentPostId].length);

                        // Xóa nội dung input
                        commentInput.value = '';
                    } catch (error) {
                        console.error("Lỗi khi phân tích phản hồi:", error);
                    }
                } else {
                    console.error("Lỗi khi thêm bình luận:", xhr.responseText);
                    alert("Không thể thêm bình luận. Vui lòng thử lại sau.");
                }
            }
        };

        xhr.send(JSON.stringify(commentData));
    }
}

function createCommentElement(comment) {
    const commentItem = document.createElement('div');
    commentItem.className = 'fb-comment-item';

    const commentDate = comment.createdAt ? new Date(comment.createdAt) : new Date();
    const formattedDate = commentDate.toLocaleString();

    commentItem.innerHTML = `
                <div class="comment-avatar">
                    <img src="~/img/avatar.jpg" alt="avatar" class="comment-user-avatar">
                </div>
                <div class="fb-comment-content">
                    <div class="fb-comment-author">${comment.userName || 'User'}</div>
                    <div class="fb-comment-text">${comment.content}</div>
                    <div class="fb-comment-time">${formattedDate}</div>
                    <div class="fb-comment-actions">
                        <span class="fb-comment-action">Like</span>
                        <span class="fb-comment-action">Reply</span>
                    </div>
                </div>
            `;

    return commentItem;
}

document.addEventListener('DOMContentLoaded', function () {
    // Load all comment counts when page loads
    loadAllCommentCounts();

    // Handle Enter key for comments
    document.querySelectorAll('.fb-comment-input').forEach(input => {
        input.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                submitComment({ target: this.closest('.fb-comment-input-wrapper').querySelector('.fa-paper-plane') });
            }
        });
    });

    // Close comment overlay when clicked outside
    document.addEventListener('click', function (event) {
        const commentOverlay = document.getElementById('comment-overlay');
        if (commentOverlay.style.display === 'flex' &&
            !event.target.closest('.comment-container') &&
            !event.target.closest('.comment-btn')) {
            closeCommentSection();
        }
    });

    // Initialize audio players
    const customPlayers = document.querySelectorAll('.custom-audio-player');

    customPlayers.forEach(player => {
        const songUrl = player.getAttribute('data-song-url');
        const playBtn = player.querySelector('.play-button');
        const playIcon = player.querySelector('.play-button i');
        const progressContainer = player.querySelector('.progress-container');
        const progressBar = player.querySelector('.progress-bar');
        const timeDisplay = player.querySelector('.time-display');
        const audio = player.querySelector('audio');

        function formatTime(seconds) {
            const minutes = Math.floor(seconds / 60);
            const secs = Math.floor(seconds % 60);
            return `${minutes}:${secs < 10 ? '0' + secs : secs}`;
        }

        audio.addEventListener('timeupdate', function () {
            const progress = (audio.currentTime / audio.duration) * 100;
            progressBar.style.width = `${progress}%`;
            timeDisplay.textContent = formatTime(audio.currentTime);
        });

        audio.addEventListener('loadedmetadata', function () {
            timeDisplay.textContent = formatTime(audio.duration);
        });

        playBtn.addEventListener('click', function () {
            if (audio.paused) {
                document.querySelectorAll('audio').forEach(a => {
                    if (a !== audio && !a.paused) {
                        a.pause();

                        const otherPlayer = a.closest('.custom-audio-player');
                        if (otherPlayer) {
                            const otherIcon = otherPlayer.querySelector('.play-button i');
                            otherIcon.classList.remove('fa-pause');
                            otherIcon.classList.add('fa-play');
                        }
                    }
                });

                audio.play();
                playIcon.classList.remove('fa-play');
                playIcon.classList.add('fa-pause');
            } else {
                audio.pause();
                playIcon.classList.remove('fa-pause');
                playIcon.classList.add('fa-play');
            }
        });

        // Click on progress bar to seek
        progressContainer.addEventListener('click', function (e) {
            const rect = this.getBoundingClientRect();
            const clickPosition = (e.clientX - rect.left) / rect.width;
            audio.currentTime = clickPosition * audio.duration;
        });

        // Reset when audio ends
        audio.addEventListener('ended', function () {
            audio.currentTime = 0;
            playIcon.classList.remove('fa-pause');
            playIcon.classList.add('fa-play');
        });
    });

    // Volume slider functionality
    document.querySelectorAll(".volume-slider").forEach(slider => {
        slider.addEventListener("input", function () {
            let songId = this.id.replace("volume-slider-", ""); // Lấy ID bài hát từ thanh trượt
            let audio = document.getElementById(`audio-${songId}`);
            if (audio) {
                audio.volume = this.value;
            }
        });
    });
});
document.addEventListener('DOMContentLoaded', function () {
    // Đảm bảo tất cả các sự kiện được gắn khi DOM đã tải
    loadAllCommentCounts();

    // Đảm bảo rằng nút submit comment xử lý sự kiện nhập phím 'Enter'
    document.querySelectorAll('.fb-comment-input').forEach(input => {
        input.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                submitComment({ target: this.closest('.fb-comment-input-wrapper').querySelector('.fa-paper-plane') });
            }
        });
    });

    // Đóng overlay khi click ngoài
    document.addEventListener('click', function (event) {
        const commentOverlay = document.getElementById('comment-overlay');
        if (commentOverlay.style.display === 'flex' &&
            !event.target.closest('.comment-container') &&
            !event.target.closest('.comment-btn')) {
            closeCommentSection();
        }
    });

    // Các hàm xử lý các nút khác...
});