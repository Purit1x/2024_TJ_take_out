<!-- 8-12 吴昊泽 首页界面更新 -->
<template>
  <div class="container">
    <img class="background" src="@/assets/index_bg.png" alt="Background" />
    <div class="overlay">
      <router-link to="/login" class="link-button"><img src="@/assets/index_button.png" /></router-link>
      <img class="cover" src="@/assets/index_cover.png" alt="Cover" />
    </div>
  </div>
</template>

<style scoped lang="scss">
.container {
  position: relative;
  width: 100vw;
  height: 100vh;
  overflow: hidden;
}

.background {
  width: 100%;
  height: 100%;
  object-fit: cover;
  position: absolute;
  top: 0;
  left: 0;
  z-index: 1;
}

.overlay {
  position: relative;
  z-index: 2;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
}

.cover {
  width: 90%;
  height: auto;
  position: absolute;
  top: 25%;
}

.link-button{
  position: absolute;
  z-index: 3;
  display: flex;
  justify-content: center;
  align-items: center;
  top: 62%;
  left: 18%;
}

</style>