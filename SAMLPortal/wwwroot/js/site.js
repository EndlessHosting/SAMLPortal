﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function getResultsFromFilters()
{
	let uri = "/api/setup/getResultsFromFilters";
	let testUsername = document.getElementById("testUsername").value;

	fetch(uri, {
		method: "POST",
		headers: {
			"Content-Type" : "application/json"
		},
		body: JSON.stringify(testUsername)
	})
	.then((response) => response.json())
	.then((response) => {
		let values = response.value;

		if(Object.keys(values).length > 0)
		{
			let resultsDiv = document.getElementById("resultsDiv");
			resultsDiv.innerHTML = "";
			
			let users = values["0"];
			let admins = values["1"];

			resultsDiv.innerHTML += "<p>Results of User filter :</p>";

			for (let userKey in users)
			{
				let value = values[userKey];
				resultsDiv.innerHTML += "<div class='alert alert-primary' role='alert'>" + value + "</div>";
			}

			resultsDiv.innerHTML += "<p>Results of Administrators filter :</p>";

			for (let adminKey in admins)
			{
				let value = values[adminKey];
				resultsDiv.innerHTML += "<div class='alert alert-primary' role='alert'>" + value + "</div>";
			}
		}
	});
}