const form = document.getElementById("scholarshipForm");
const message = document.getElementById("message");

form.addEventListener("submit", async (event) => {
    event.preventDefault();

    const token = localStorage.getItem("token");

    if (!token) {
        showMessage("You must be logged in as an administrator.", "danger");
        return;
    }

    const scholarship = {
        name: document.getElementById("name").value,
        description: document.getElementById("description").value,
        deadline: document.getElementById("deadline").value,
        eligibility: document.getElementById("eligibility").value,
        howToApply: document.getElementById("howToApply").value,
        officialUrl: document.getElementById("officialUrl").value,
        requiredDocuments: document.getElementById("requiredDocuments").value
    };

    try {

        const response = await fetch(
            "http://localhost:5274/scholarships",
            {
                method: "POST",

                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },

                body: JSON.stringify(scholarship)
            }
        );

        if (response.ok) {

            showMessage(
                "Scholarship created successfully!",
                "success"
            );

            form.reset();

            return;
        }

        if (response.status === 401) {
            showMessage(
                "You are not authenticated.",
                "danger"
            );

            return;
        }

        if (response.status === 403) {
            showMessage(
                "You do not have permission to create scholarships.",
                "danger"
            );

            return;
        }

        const error = await response.json();

        showMessage(
            error.title ?? "Something went wrong.",
            "danger"
        );

    } catch (error) {

        console.error(error);

        showMessage(
            "Could not connect to the API.",
            "danger"
        );
    }
});


function showMessage(text, type) {

    message.innerHTML = `
        <div class="alert alert-${type}">
            ${text}
        </div>
    `;
}