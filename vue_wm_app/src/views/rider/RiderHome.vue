<script setup>
    // import '@/assets/rider/css/home.css' //导入样式
    import RiderHomeHeader from '@/components/rider/home/Header.vue' //导入 Header 组件
	import RiderHomeSide from '@/components/rider/home/Side.vue' //导入侧边栏组件
</script>

<template>
    <div class="home">
        <RiderHomeHeader />
        <div class="main">
            <RiderHomeSide />
            <div class="ridercontent">
                <router-view />
            </div>
        </div>        
    </div>
</template>

<style scoped>
.main {
    display: flex;
    height: calc(100vh - 70px); /* 减去 header 高度 */
}

.ridercontent {
    padding: 5px;
    flex: 1;
    overflow-y: auto;
}

</style>
