function getByValue(arr, key, value) {
	alert(arr);
	for (var i = 0; i < arr.length; i++) {
		alert(arr[i]);
		if (arr[i][key] === value)
			return arr[i];
	}
}

function FormatDateTime(date) {
	return AddTrailingZeros(date.getDate(), 2) + "." + AddTrailingZeros(date.getMonth() + 1, 2) + "." + date.getFullYear() + " " + AddTrailingZeros(date.getHours(), 2) + ":" + AddTrailingZeros(date.getMinutes(), 2);
}

function AddTrailingZeros(str, howmany) {
	str = "00000000000000000000000000000000" + str;
	return str.substring(str.length - howmany, str.length);
}