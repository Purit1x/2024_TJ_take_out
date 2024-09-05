<script setup>

</script>

<template>
    <div class="side">
        <el-menu router background-color="#545c64" text-color="#fff" active-text-color="#ffd04b">
            <el-menu-item index="/rider-home/assignment">
                <el-icon><Files /></el-icon> 订单管理
            </el-menu-item>
            <el-menu-item index="/rider-home/wage">
                <el-icon><Coin /></el-icon> 工资记录
            </el-menu-item>
            <el-menu-item index="/rider-home/information">
                <el-icon><User /></el-icon> 个人主页
            </el-menu-item>
        </el-menu>
    </div>
</template>

<style scoped>
.side {
    width: 150px;
    background: #545c64;
}

</style>