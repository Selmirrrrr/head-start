/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Components/**/*.{html,js,cshtml,razor}",
        "./Pages/**/*.{html,js,cshtml,razor}",
        "./Layout/**/*.{html,js,cshtml,razor}",
        "./*.{html,js,cshtml,razor}",
        "./wwwroot/**/*.{html,js}"
    ],
    plugins: [],
}