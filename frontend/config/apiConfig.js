const getApiBase = () => {
  if (window.location.protocol === "file:" || ["5500", "3000", "5173", "8080"].includes(window.location.port)) {
    return localStorage.getItem("geomac_api_url") || "http://localhost:5235/api";
  }
  return "/api";
};
