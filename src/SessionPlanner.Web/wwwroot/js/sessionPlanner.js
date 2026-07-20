window.sessionPlanner = {
  storage: {
    get: (key) => sessionStorage.getItem(key),
    set: (key, value) => sessionStorage.setItem(key, value),
    remove: (key) => sessionStorage.removeItem(key)
  },
  downloadFromBytes: (fileName, contentType, base64Data) => {
    const link = document.createElement("a");
    link.download = fileName;
    link.href = `data:${contentType};base64,${base64Data}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
};
